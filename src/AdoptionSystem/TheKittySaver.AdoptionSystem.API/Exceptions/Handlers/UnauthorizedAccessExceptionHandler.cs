using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for UnauthorizedAccessException and ForbiddenAccessException (403 Forbidden).
/// </summary>
internal sealed class UnauthorizedAccessExceptionHandler : IExceptionHandler
{
    private readonly ILogger<UnauthorizedAccessExceptionHandler> _logger;

    public UnauthorizedAccessExceptionHandler(ILogger<UnauthorizedAccessExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not (UnauthorizedAccessException or ForbiddenAccessException))
        {
            return false;
        }

        _logger.LogWarning(
            "Forbidden access attempt. CorrelationId: {CorrelationId}, User: {User}, Path: {Path}",
            httpContext.TraceIdentifier,
            httpContext.User.Identity?.Name ?? "Anonymous",
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden",
            Detail = exception is ForbiddenAccessException forbiddenEx
                ? forbiddenEx.Message
                : "You do not have permission to access this resource.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = exception is ForbiddenAccessException fex
            ? fex.ErrorCode
            : "FORBIDDEN";

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

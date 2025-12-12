using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for UnauthenticatedException (401 Unauthorized).
/// </summary>
internal sealed class UnauthenticatedExceptionHandler : IExceptionHandler
{
    private readonly ILogger<UnauthenticatedExceptionHandler> _logger;

    public UnauthenticatedExceptionHandler(ILogger<UnauthenticatedExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not UnauthenticatedException unauthenticatedException)
        {
            return false;
        }

        _logger.LogWarning(
            "Unauthenticated access attempt. CorrelationId: {CorrelationId}, Path: {Path}",
            httpContext.TraceIdentifier,
            httpContext.Request.Path);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = unauthenticatedException.Message,
            Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = unauthenticatedException.ErrorCode;

        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

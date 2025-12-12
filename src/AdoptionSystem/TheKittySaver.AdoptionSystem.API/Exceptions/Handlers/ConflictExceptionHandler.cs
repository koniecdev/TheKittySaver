using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for ConflictException (409 Conflict).
/// </summary>
internal sealed class ConflictExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ConflictExceptionHandler> _logger;

    public ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ConflictException conflictException)
        {
            return false;
        }

        _logger.LogWarning(
            "Conflict occurred. CorrelationId: {CorrelationId}, Message: {Message}",
            httpContext.TraceIdentifier,
            conflictException.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Title = "Conflict",
            Detail = conflictException.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = conflictException.ErrorCode;

        httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

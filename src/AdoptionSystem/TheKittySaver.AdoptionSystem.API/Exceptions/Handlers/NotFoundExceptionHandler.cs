using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for NotFoundException (404 Not Found).
/// </summary>
internal sealed class NotFoundExceptionHandler : IExceptionHandler
{
    private readonly ILogger<NotFoundExceptionHandler> _logger;

    public NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not NotFoundException notFoundException)
        {
            return false;
        }

        _logger.LogWarning(
            "Resource not found. CorrelationId: {CorrelationId}, Resource: {Resource}, Id: {ResourceId}",
            httpContext.TraceIdentifier,
            notFoundException.ResourceName,
            notFoundException.ResourceId);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Not Found",
            Detail = notFoundException.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = notFoundException.ErrorCode;

        if (!string.IsNullOrEmpty(notFoundException.ResourceName))
        {
            problemDetails.Extensions["resourceName"] = notFoundException.ResourceName;
        }

        if (notFoundException.ResourceId is not null)
        {
            problemDetails.Extensions["resourceId"] = notFoundException.ResourceId.ToString();
        }

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TheKittySaver.AdoptionSystem.API.ExceptionHandlers;

internal sealed class ArgumentExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ArgumentExceptionHandler> _logger;

    public ArgumentExceptionHandler(ILogger<ArgumentExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ArgumentException argumentException)
        {
            return false;
        }

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
            Title = "Bad Request",
            Detail = argumentException.Message
        };

        _logger.LogWarning(
            argumentException,
            "Code:{@ErrorCode},Type:{@ErrorType},Message:{@ErrorMessage}",
            argumentException.GetType().Name,
            "ArgumentException",
            argumentException.Message);

        httpContext.Response.StatusCode = problemDetails.Status.Value;

        await httpContext.Response
            .WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

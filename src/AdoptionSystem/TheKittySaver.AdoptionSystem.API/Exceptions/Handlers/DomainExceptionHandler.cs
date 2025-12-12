using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for domain exceptions (ArgumentException, InvalidOperationException) → 400 Bad Request.
/// </summary>
internal sealed class DomainExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public DomainExceptionHandler(
        ILogger<DomainExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not (ArgumentException or InvalidOperationException or BadRequestException))
        {
            return false;
        }

        _logger.LogWarning(
            exception,
            "Domain exception occurred. CorrelationId: {CorrelationId}, ExceptionType: {ExceptionType}",
            httpContext.TraceIdentifier,
            exception.GetType().Name);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = exception.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;

        var errorCode = exception switch
        {
            BadRequestException badRequest => badRequest.ErrorCode,
            ArgumentException => "ARGUMENT_ERROR",
            InvalidOperationException => "INVALID_OPERATION",
            _ => "BAD_REQUEST"
        };

        problemDetails.Extensions["errorCode"] = errorCode;

        if (_environment.IsDevelopment() && exception is ArgumentException argEx)
        {
            problemDetails.Extensions["paramName"] = argEx.ParamName;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

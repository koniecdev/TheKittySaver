using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for FluentValidation exceptions (400 Bad Request).
/// </summary>
internal sealed class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not FluentValidation.ValidationException validationException)
        {
            return false;
        }

        _logger.LogWarning(
            "Validation failed. CorrelationId: {CorrelationId}, Errors: {ErrorCount}",
            httpContext.TraceIdentifier,
            validationException.Errors.Count());

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var problemDetails = new ValidationProblemDetails(errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred.",
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = "VALIDATION_ERROR";

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

/// <summary>
/// Exception handler for custom domain validation exceptions (400 Bad Request).
/// </summary>
internal sealed class DomainValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DomainValidationExceptionHandler> _logger;

    public DomainValidationExceptionHandler(ILogger<DomainValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not Exceptions.ValidationException validationException)
        {
            return false;
        }

        _logger.LogWarning(
            "Domain validation failed. CorrelationId: {CorrelationId}, ErrorCode: {ErrorCode}",
            httpContext.TraceIdentifier,
            validationException.ErrorCode);

        var problemDetails = new ValidationProblemDetails(
            validationException.Errors.ToDictionary(k => k.Key, v => v.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Detail = validationException.Message,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = validationException.ErrorCode;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}

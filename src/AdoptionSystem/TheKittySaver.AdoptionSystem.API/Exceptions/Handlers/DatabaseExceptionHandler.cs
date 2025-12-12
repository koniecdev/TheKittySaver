using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for database-related exceptions (DbUpdateException, DbUpdateConcurrencyException).
/// </summary>
internal sealed class DatabaseExceptionHandler : IExceptionHandler
{
    private readonly ILogger<DatabaseExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public DatabaseExceptionHandler(
        ILogger<DatabaseExceptionHandler> logger,
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
        if (exception is not (DbUpdateException or DbUpdateConcurrencyException))
        {
            return false;
        }

        _logger.LogError(
            exception,
            "Database operation failed. CorrelationId: {CorrelationId}, ExceptionType: {ExceptionType}",
            httpContext.TraceIdentifier,
            exception.GetType().Name);

        var (statusCode, title, detail, errorCode) = exception switch
        {
            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Concurrency Conflict",
                "The resource was modified by another user. Please refresh and try again.",
                "CONCURRENCY_CONFLICT"),
            DbUpdateException dbEx when IsDuplicateKeyException(dbEx) => (
                StatusCodes.Status409Conflict,
                "Duplicate Entry",
                "A resource with the same key already exists.",
                "DUPLICATE_KEY"),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Database Error",
                _environment.IsDevelopment()
                    ? exception.InnerException?.Message ?? exception.Message
                    : "A database error occurred. Please try again later.",
                "DATABASE_ERROR")
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Type = GetTypeUri(statusCode),
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["correlationId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["errorCode"] = errorCode;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static bool IsDuplicateKeyException(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("unique constraint", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("UNIQUE KEY", StringComparison.OrdinalIgnoreCase) ||
               message.Contains("PRIMARY KEY", StringComparison.OrdinalIgnoreCase);
    }

    private static string GetTypeUri(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status409Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
}

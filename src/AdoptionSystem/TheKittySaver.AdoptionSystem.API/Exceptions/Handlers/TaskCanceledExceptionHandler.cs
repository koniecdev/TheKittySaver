using Microsoft.AspNetCore.Diagnostics;

namespace TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

/// <summary>
/// Exception handler for TaskCanceledException and OperationCanceledException.
/// These typically occur when the client disconnects or request times out.
/// </summary>
internal sealed class TaskCanceledExceptionHandler : IExceptionHandler
{
    private readonly ILogger<TaskCanceledExceptionHandler> _logger;

    public TaskCanceledExceptionHandler(ILogger<TaskCanceledExceptionHandler> logger)
    {
        _logger = logger;
    }

    public ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not (TaskCanceledException or OperationCanceledException))
        {
            return ValueTask.FromResult(false);
        }

        // Check if the request was actually cancelled by the client
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogDebug(
                "Request cancelled by client. CorrelationId: {CorrelationId}, Path: {Path}",
                httpContext.TraceIdentifier,
                httpContext.Request.Path);

            // Don't send a response - client has disconnected
            return ValueTask.FromResult(true);
        }

        _logger.LogWarning(
            exception,
            "Operation timed out. CorrelationId: {CorrelationId}, Path: {Path}",
            httpContext.TraceIdentifier,
            httpContext.Request.Path);

        // Let the global handler deal with actual timeouts
        return ValueTask.FromResult(false);
    }
}

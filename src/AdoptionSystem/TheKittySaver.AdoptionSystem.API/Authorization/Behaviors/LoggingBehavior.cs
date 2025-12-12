using System.Diagnostics;
using Mediator;

namespace TheKittySaver.AdoptionSystem.API.Authorization.Behaviors;

/// <summary>
/// Mediator pipeline behavior that logs request execution.
/// </summary>
/// <typeparam name="TMessage">The type of message being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
internal sealed class LoggingBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<LoggingBehavior<TMessage, TResponse>> _logger;

    public LoggingBehavior(
        ICurrentUserService currentUserService,
        ILogger<LoggingBehavior<TMessage, TResponse>> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        var requestName = typeof(TMessage).Name;
        var userId = _currentUserService.UserId ?? "Anonymous";

        _logger.LogDebug(
            "Handling {RequestName} for user {UserId}",
            requestName,
            userId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(message, cancellationToken);

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds > 500)
            {
                _logger.LogWarning(
                    "Long running request: {RequestName} ({ElapsedMilliseconds} ms) for user {UserId}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    userId);
            }
            else
            {
                _logger.LogDebug(
                    "Completed {RequestName} in {ElapsedMilliseconds} ms for user {UserId}",
                    requestName,
                    stopwatch.ElapsedMilliseconds,
                    userId);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Request {RequestName} failed after {ElapsedMilliseconds} ms for user {UserId}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                userId);

            throw;
        }
    }
}

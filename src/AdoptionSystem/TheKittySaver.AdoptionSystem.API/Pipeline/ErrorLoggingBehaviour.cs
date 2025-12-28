using Mediator;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.API.Pipeline;

#pragma warning disable CA1515
public sealed class FailureLoggingBehaviour<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
    where TResponse : Result
{
    private readonly ILogger<FailureLoggingBehaviour<TMessage, TResponse>> _logger;

    public FailureLoggingBehaviour(ILogger<FailureLoggingBehaviour<TMessage, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = await next(message, cancellationToken);

        if (response.IsFailure)
        {
            _logger.LogError("Code:{@ErrorCode},Type:{@ErrorType},Message:{@ErrorMessage}",
                response.Error.Code,
                response.Error.Type,
                response.Error.Message);
        }

        return response;
    }
}
#pragma warning restore CA1515

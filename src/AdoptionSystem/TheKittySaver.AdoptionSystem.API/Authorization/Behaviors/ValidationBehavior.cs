using FluentValidation;
using Mediator;

namespace TheKittySaver.AdoptionSystem.API.Authorization.Behaviors;

/// <summary>
/// Mediator pipeline behavior that handles validation using FluentValidation.
/// </summary>
/// <typeparam name="TMessage">The type of message being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
internal sealed class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;
    private readonly ILogger<ValidationBehavior<TMessage, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TMessage>> validators,
        ILogger<ValidationBehavior<TMessage, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        if (!_validators.Any())
        {
            return await next(message, cancellationToken);
        }

        var context = new ValidationContext<TMessage>(message);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
        {
            _logger.LogWarning(
                "Validation failed for {RequestType}. Errors: {ErrorCount}",
                typeof(TMessage).Name,
                failures.Count);

            throw new ValidationException(failures);
        }

        return await next(message, cancellationToken);
    }
}

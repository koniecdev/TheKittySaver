using Mediator;
using TheKittySaver.AdoptionSystem.API.Exceptions;

namespace TheKittySaver.AdoptionSystem.API.Authorization.Behaviors;

/// <summary>
/// Mediator pipeline behavior that handles authorization checks.
/// </summary>
/// <typeparam name="TMessage">The type of message being processed.</typeparam>
/// <typeparam name="TResponse">The type of response returned.</typeparam>
internal sealed class AuthorizationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuthorizationBehavior<TMessage, TResponse>> _logger;

    public AuthorizationBehavior(
        ICurrentUserService currentUserService,
        ILogger<AuthorizationBehavior<TMessage, TResponse>> logger)
    {
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message,
        CancellationToken cancellationToken,
        MessageHandlerDelegate<TMessage, TResponse> next)
    {
        // If the message doesn't require authorization, skip
        if (message is not IAuthorizedRequest)
        {
            return await next(message, cancellationToken);
        }

        // Check if user is authenticated
        if (!_currentUserService.IsAuthenticated)
        {
            _logger.LogWarning(
                "Unauthenticated access attempt to {RequestType}",
                typeof(TMessage).Name);

            throw new UnauthenticatedException();
        }

        // Check admin-only requests
        if (message is IAdminOnlyRequest)
        {
            if (!_currentUserService.IsInRole(Roles.Admin))
            {
                _logger.LogWarning(
                    "Unauthorized admin-only access attempt by user {UserId} to {RequestType}",
                    _currentUserService.UserId,
                    typeof(TMessage).Name);

                throw new ForbiddenAccessException(
                    "This operation requires administrator privileges.");
            }
        }

        // Check job-or-admin requests
        if (message is IJobOrAdminOnlyRequest)
        {
            var isJobUser = _currentUserService.IsInRole(Roles.Job);
            var isAdmin = _currentUserService.IsInRole(Roles.Admin);

            if (!isJobUser && !isAdmin)
            {
                _logger.LogWarning(
                    "Unauthorized job/admin access attempt by user {UserId} to {RequestType}",
                    _currentUserService.UserId,
                    typeof(TMessage).Name);

                throw new ForbiddenAccessException(
                    "This operation requires job or administrator privileges.");
            }
        }

        _logger.LogDebug(
            "Authorization successful for user {UserId} on {RequestType}",
            _currentUserService.UserId,
            typeof(TMessage).Name);

        return await next(message, cancellationToken);
    }
}

/// <summary>
/// Defines role constants used for authorization.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Job = "Job";
    public const string Moderator = "Moderator";
}

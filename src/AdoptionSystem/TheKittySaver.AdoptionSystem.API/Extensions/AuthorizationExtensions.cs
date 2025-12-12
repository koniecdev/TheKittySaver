using TheKittySaver.AdoptionSystem.API.Authorization;

namespace TheKittySaver.AdoptionSystem.API.Extensions;

/// <summary>
/// Extension methods for configuring authorization services.
/// </summary>
public static class AuthorizationExtensions
{
    /// <summary>
    /// Adds authorization services to the service collection.
    /// </summary>
    public static IServiceCollection AddAuthorizationConfiguration(this IServiceCollection services)
    {
        // Add HTTP context accessor for accessing the current user
        services.AddHttpContextAccessor();

        // Register the current user service
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Note: Pipeline behaviors are registered via Mediator source generator
        // The AuthorizationBehavior, ValidationBehavior, and LoggingBehavior
        // will be automatically discovered and applied to the pipeline

        return services;
    }

    /// <summary>
    /// Configures authentication and authorization middleware.
    /// Call this after app.UseRouting() and before app.MapEndpoints().
    /// </summary>
    public static WebApplication UseAuthorizationConfiguration(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}

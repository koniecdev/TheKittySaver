using TheKittySaver.AdoptionSystem.API.Exceptions.Handlers;

namespace TheKittySaver.AdoptionSystem.API.Extensions;

/// <summary>
/// Extension methods for configuring exception handling in the application.
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Adds all exception handlers to the service collection.
    /// The order of registration determines the order in which handlers are tried.
    /// More specific handlers should be registered first.
    /// </summary>
    public static IServiceCollection AddExceptionHandlers(this IServiceCollection services)
    {
        // Register handlers from most specific to least specific
        // The first handler that returns true will handle the exception

        // Cancellation handlers (client disconnect, timeout)
        services.AddExceptionHandler<TaskCanceledExceptionHandler>();

        // Authentication & Authorization (401, 403)
        services.AddExceptionHandler<UnauthenticatedExceptionHandler>();
        services.AddExceptionHandler<UnauthorizedAccessExceptionHandler>();

        // Validation handlers (400)
        services.AddExceptionHandler<ValidationExceptionHandler>();       // FluentValidation
        services.AddExceptionHandler<DomainValidationExceptionHandler>(); // Custom validation

        // Business logic handlers (400, 404, 409)
        services.AddExceptionHandler<NotFoundExceptionHandler>();
        services.AddExceptionHandler<ConflictExceptionHandler>();
        services.AddExceptionHandler<DomainExceptionHandler>();

        // Database handlers (409, 500)
        services.AddExceptionHandler<DatabaseExceptionHandler>();

        // Global fallback handler (500) - must be last
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // Configure ProblemDetails for consistent error responses
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = context.HttpContext.Request.Path;

                // Always add correlation ID
                if (!context.ProblemDetails.Extensions.ContainsKey("correlationId"))
                {
                    context.ProblemDetails.Extensions["correlationId"] = context.HttpContext.TraceIdentifier;
                }

                // Add timestamp
                context.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow.ToString("O");
            };
        });

        return services;
    }
}

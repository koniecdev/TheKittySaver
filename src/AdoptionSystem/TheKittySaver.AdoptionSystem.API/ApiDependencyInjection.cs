using TheKittySaver.AdoptionSystem.API.ExceptionHandlers;
using TheKittySaver.AdoptionSystem.API.Pipeline;

namespace TheKittySaver.AdoptionSystem.API;

internal static class ApiDependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddExceptionHandler<ArgumentExceptionHandler>();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(FailureLoggingBehaviour<,>)];
        });


        return services;
    }
}

using TheKittySaver.AdoptionSystem.API.DomainEventHandlers;
using TheKittySaver.AdoptionSystem.API.ExceptionHandlers;
using TheKittySaver.AdoptionSystem.API.Interceptors;
using TheKittySaver.AdoptionSystem.API.Pipeline;

namespace TheKittySaver.AdoptionSystem.API;

internal static class ApiDependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();
        
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
            options.PipelineBehaviors = [typeof(FailureLoggingBehaviour<,>)];
        });

        services.AddSingleton<IDomainEventPublisher, MediatorDomainEventPublisher>();
        services.AddSingleton<PublishDomainEventsInterceptor>();

        return services;
    }
}

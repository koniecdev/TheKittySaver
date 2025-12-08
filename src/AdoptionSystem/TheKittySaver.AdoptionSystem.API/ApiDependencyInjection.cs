using TheKittySaver.AdoptionSystem.API.DomainEventHandlers;
using TheKittySaver.AdoptionSystem.API.Interceptors;

namespace TheKittySaver.AdoptionSystem.API;

internal static class ApiDependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        services.AddScoped<IDomainEventPublisher, MediatorDomainEventPublisher>();
        services.AddScoped<PublishDomainEventsInterceptor>();

        return services;
    }
}

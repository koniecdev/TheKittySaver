using FluentValidation;
using TheKittySaver.AdoptionSystem.API.Authorization;
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

        // Authorization
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // FluentValidation - auto-register all validators from this assembly
        services.AddValidatorsFromAssemblyContaining<ApiDependencyInjection>(ServiceLifetime.Scoped);

        return services;
    }
}

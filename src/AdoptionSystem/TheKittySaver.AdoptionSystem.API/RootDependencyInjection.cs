using Microsoft.EntityFrameworkCore.Diagnostics;
using TheKittySaver.AdoptionSystem.API.Interceptors;
using TheKittySaver.AdoptionSystem.Domain;
using TheKittySaver.AdoptionSystem.Infrastructure;
using TheKittySaver.AdoptionSystem.Persistence;

namespace TheKittySaver.AdoptionSystem.API;

internal static class RootDependencyInjection
{
    public static IServiceCollection Register(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddDomain();
        services.AddApi();
        services.AddPersistence(GetInterceptors);

        return services;
    }

    private static IEnumerable<IInterceptor> GetInterceptors(IServiceProvider serviceProvider)
    {
        yield return serviceProvider.GetRequiredService<PublishDomainEventsInterceptor>();
    }
}

using TheKittySaver.AdoptionSystem.Domain;
using TheKittySaver.AdoptionSystem.Infrastructure;
using TheKittySaver.AdoptionSystem.Persistence;

namespace TheKittySaver.AdoptionSystem.API;

internal static class RootDependencyInjection
{
    public static IServiceCollection Register(this IServiceCollection services)
    {
        services.AddInfrastructure();
        services.AddDomain();
        services.AddPersistence();
        services.AddApi();

        return services;
    }
}

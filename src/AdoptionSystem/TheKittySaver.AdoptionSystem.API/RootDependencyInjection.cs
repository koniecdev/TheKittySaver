using TheKittySaver.AdoptionSystem.Calculators;
using TheKittySaver.AdoptionSystem.Domain;
using TheKittySaver.AdoptionSystem.Infrastructure;
using TheKittySaver.AdoptionSystem.Persistence;

namespace TheKittySaver.AdoptionSystem.API;

internal static class RootDependencyInjection
{
    public static IServiceCollection AddAdoptionSystem(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCalculators(configuration);
        services.AddDomain();
        services.AddInfrastructure(configuration);
        services.AddPersistence();
        services.AddApi();

        return services;
    }
}

using TheKittySaver.AdoptionSystem.Persistence;

namespace TheKittySaver.AdoptionSystem.API;

internal static class RootDependencyInjection
{
    public static IServiceCollection Register(this IServiceCollection services)
    {
        services.AddPersistence();
        services.AddApi();
        
        return services;
    }
}

namespace TheKittySaver.AdoptionSystem.API;

internal static class ApiDependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddMediator();

        return services;
    }
}

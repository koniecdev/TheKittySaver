using FluentValidation;
using TheKittySaver.AdoptionSystem.API.DependencyInjectionExtensions;
using TheKittySaver.AdoptionSystem.API.Settings.ConnectionStringsOption;

namespace TheKittySaver.AdoptionSystem.API;

internal static class OptionsDependencyInjection
{
    public static IServiceCollection RegisterOptions(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        
        services.AddOptionsWithFluentValidation<ConnectionStrings>(ConnectionStrings.ConfigurationSection);
        
        return services;
    }
}

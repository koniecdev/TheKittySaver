using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Calculators.Factories;

namespace TheKittySaver.AdoptionSystem.Calculators;

public static class CalculatorsDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCalculators(IConfiguration configuration)
        {
            services.Configure<CalculatorsOptions>(configuration.GetSection(CalculatorsOptions.SectionName));

            services.AddSingleton<IAdoptionPriorityScoreCalculatorFactory, AdoptionPriorityScoreCalculatorFactory>();

            return services;
        }
    }
}

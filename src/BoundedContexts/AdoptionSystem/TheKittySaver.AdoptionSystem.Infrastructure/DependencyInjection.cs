using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Specifications;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

namespace TheKittySaver.AdoptionSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<LibPhoneNumberValidator>();
        serviceCollection.AddScoped<IValidPhoneNumberSpecification>(sp 
            => sp.GetRequiredService<LibPhoneNumberValidator>());
        serviceCollection.AddScoped<IPhoneNumberNormalizer>(sp 
            => sp.GetRequiredService<LibPhoneNumberNormalizer>());
        return serviceCollection;
    }
}
using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Specifications;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

namespace TheKittySaver.AdoptionSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<LibPhoneNumberSpecification>();
        serviceCollection.AddScoped<IValidPhoneNumberSpecification>(sp => sp.GetRequiredService<LibPhoneNumberSpecification>());
        serviceCollection.AddScoped<IPhoneNumberNormalizer>(sp => sp.GetRequiredService<LibPhoneNumberSpecification>());
        return serviceCollection;
    }
}
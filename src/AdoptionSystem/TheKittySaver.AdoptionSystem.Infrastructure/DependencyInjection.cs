using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

namespace TheKittySaver.AdoptionSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<LibPhoneNumberValidator>();
        serviceCollection.AddScoped<IValidPhoneNumberSpecification>(sp 
            => sp.GetRequiredService<LibPhoneNumberValidator>());
        
        serviceCollection.AddScoped<LibPhoneNumberNormalizer>();
        serviceCollection.AddScoped<IPhoneNumberNormalizer>(sp 
            => sp.GetRequiredService<LibPhoneNumberNormalizer>());
        
        serviceCollection.AddScoped<PolandAddressConsistencySpecification>();
        serviceCollection.AddScoped<IAddressConsistencySpecification>(sp 
            => sp.GetRequiredService<PolandAddressConsistencySpecification>());
        
        return serviceCollection;
    }
}
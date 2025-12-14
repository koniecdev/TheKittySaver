using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

namespace TheKittySaver.AdoptionSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        serviceCollection.AddSingleton(TimeProvider.System);
        
        serviceCollection.AddScoped<LibPhoneNumberValidator>();
        serviceCollection.AddScoped<IValidPhoneNumberSpecification>(sp
            => sp.GetRequiredService<LibPhoneNumberValidator>());

        serviceCollection.AddScoped<LibPhoneNumberNormalizer>();
        serviceCollection.AddScoped<IPhoneNumberNormalizer>(sp
            => sp.GetRequiredService<LibPhoneNumberNormalizer>());

        serviceCollection.AddScoped<PolandAddressConsistencySpecification>();
        serviceCollection.AddScoped<IAddressConsistencySpecification>(sp
            => sp.GetRequiredService<PolandAddressConsistencySpecification>());

        serviceCollection.Configure<CatFileStorageOptions>(
            configuration.GetSection(CatFileStorageOptions.SectionName));
        serviceCollection.AddScoped<ICatFileStorage, CatFileStorage>();

        return serviceCollection;
    }
}

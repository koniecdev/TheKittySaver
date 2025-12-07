using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain;

public static class DomainDependencyInjection
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddScoped<IPhoneNumberFactory, PhoneNumberFactory>();
        services.AddScoped<IPersonCreationService, PersonCreationService>();
        services.AddScoped<IPersonUpdateService, PersonUpdateService>();

        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain;

public static class DomainDependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddScoped<IPhoneNumberFactory, PhoneNumberFactory>();
        services.AddScoped<IPersonUniquenessCheckerService, PersonUniquenessCheckerService>(); //persistence
        services.AddScoped<IPersonCreationService, PersonCreationService>();
        
        
        //todo: puścić prompt na całe ogarnięcie DI
    }
}

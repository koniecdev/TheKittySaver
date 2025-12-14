using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.Services.AdoptionAnnouncementCreationServices;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain;

public static class DomainDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDomain()
        {
            services.AddScoped<IPhoneNumberFactory, PhoneNumberFactory>();
            services.AddScoped<IPersonCreationService, PersonCreationService>();
            services.AddScoped<IPersonUpdateService, PersonUpdateService>();
            services.AddScoped<ICatAdoptionAnnouncementAssignmentService, CatAdoptionAnnouncementAssignmentService>();
            services.AddScoped<IAdoptionAnnouncementCreationService, AdoptionAnnouncementCreationService>();

            return services;
        }
    }
}

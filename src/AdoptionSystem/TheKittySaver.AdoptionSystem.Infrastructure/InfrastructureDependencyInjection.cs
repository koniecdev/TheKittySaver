using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Infrastructure.FileStorage;
using TheKittySaver.AdoptionSystem.Infrastructure.FileUpload;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

namespace TheKittySaver.AdoptionSystem.Infrastructure;

public static class InfrastructureDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services.AddSingleton(TimeProvider.System);

            services.AddScoped<LibPhoneNumberValidator>();
            services.AddScoped<IValidPhoneNumberSpecification>(sp => sp.GetRequiredService<LibPhoneNumberValidator>());

            services.AddScoped<LibPhoneNumberNormalizer>();
            services.AddScoped<IPhoneNumberNormalizer>(sp => sp.GetRequiredService<LibPhoneNumberNormalizer>());

            services.AddScoped<PolandAddressConsistencySpecification>();
            services.AddScoped<IAddressConsistencySpecification>(sp
                => sp.GetRequiredService<PolandAddressConsistencySpecification>());

            services.Configure<CatFileStorageOptions>(configuration.GetSection(CatFileStorageOptions.SectionName));
            services.AddScoped<ICatFileStorage, CatFileStorage>();

            services.Configure<FileUploadOptions>(configuration.GetSection(FileUploadOptions.SectionName));
            services.AddScoped<IFileUploadValidator, FileUploadValidator>();

            return services;
        }
    }
}

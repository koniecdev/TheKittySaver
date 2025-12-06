using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Persistence.DomainRepositories;
using TheKittySaver.AdoptionSystem.Persistence.Settings;

namespace TheKittySaver.AdoptionSystem.Persistence;

public static class PersistenceDependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<ConnectionStringSettings>, ConnectionStringSettingsValidator>();
        services.AddOptionsWithFluentValidation<ConnectionStringSettings>(ConnectionStringSettings.ConfigurationSection);

        services.AddDbContextFactory<ApplicationWriteDbContext>((sp, options) =>
            options.UseSqlServer(sp.GetRequiredService<IOptions<ConnectionStringSettings>>().Value.Database));

        services.AddDbContextFactory<ApplicationReadDbContext>((sp, options) =>
            options.UseSqlServer(sp.GetRequiredService<IOptions<ConnectionStringSettings>>().Value.Database)
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<IApplicationReadDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationReadDbContext>());

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationWriteDbContext>());

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICatRepository, CatRepository>();
        services.AddScoped<IAdoptionAnnouncementRepository, AdoptionAnnouncementRepository>();

        return services;
    }
}

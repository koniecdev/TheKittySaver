using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Services;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Persistence.DomainRepositories;
using TheKittySaver.AdoptionSystem.Persistence.DomainServices;
using TheKittySaver.AdoptionSystem.Persistence.Interceptors;
using TheKittySaver.AdoptionSystem.Persistence.Settings;

namespace TheKittySaver.AdoptionSystem.Persistence;

public static class PersistenceDependencyInjection
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider serviceProvider)
    {
        await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        ApplicationWriteDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services)
    {
        services.AddSingleton<IValidator<ConnectionStringSettings>, ConnectionStringSettingsValidator>();
        services.AddOptionsWithFluentValidation<ConnectionStringSettings>(ConnectionStringSettings.ConfigurationSection);

        services.AddScoped<DomainEventsPublishingInterceptor>();

        services.AddDbContextFactory<ApplicationWriteDbContext>((sp, options) =>
        {
            DomainEventsPublishingInterceptor interceptor = sp.GetRequiredService<DomainEventsPublishingInterceptor>();

            options.UseSqlServer(
                sp.GetRequiredService<IOptions<ConnectionStringSettings>>().Value.Database,
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null);
                    sqlOptions.CommandTimeout(30);
                })
                .AddInterceptors(interceptor);
        });

        services.AddDbContextFactory<ApplicationReadDbContext>((sp, options) =>
            options.UseSqlServer(
                    sp.GetRequiredService<IOptions<ConnectionStringSettings>>().Value.Database,
                    sqlOptions =>
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(10),
                            errorNumbersToAdd: null);
                        sqlOptions.CommandTimeout(30);
                    })
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<IApplicationReadDbContext>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationReadDbContext>());

        services.AddScoped<IUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ApplicationWriteDbContext>());

        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<ICatRepository, CatRepository>();
        services.AddScoped<IAdoptionAnnouncementRepository, AdoptionAnnouncementRepository>();

        services.AddScoped<IPersonUniquenessCheckerService, PersonUniquenessCheckerService>();

        return services;
    }
}

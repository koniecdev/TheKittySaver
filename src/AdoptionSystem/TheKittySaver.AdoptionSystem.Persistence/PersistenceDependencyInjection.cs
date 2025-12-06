using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.ReadDbContexts;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Persistence.DomainRepositories;

namespace TheKittySaver.AdoptionSystem.Persistence;

public static class PersistenceDependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("Database")
                                  ?? throw new InvalidOperationException("Connection string 'Database' not found.");

        services.AddDbContextFactory<ApplicationWriteDbContext>(options =>
            options.UseSqlServer(connectionString));
        
        services.AddDbContextFactory<ApplicationReadDbContext>(options =>
            options.UseSqlServer(connectionString)
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

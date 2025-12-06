using Microsoft.EntityFrameworkCore;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts.DesignTimeFactories;

/// <summary>
/// Design-time factory for ApplicationWriteDbContext.
/// Used by EF Core tools for migrations.
/// </summary>
/// <example>
/// Usage:
///   dotnet ef migrations add InitialCreate --project YourProject
///   dotnet ef database update --project YourProject
///   dotnet ef migrations add Test -- --environment Production
///   dotnet ef database update -- --connection "Server=...;Database=...;"
/// </example>
internal sealed class ApplicationWriteDbContextFactory : DesignTimeDbContextFactoryBase<ApplicationWriteDbContext>
{
    protected override ApplicationWriteDbContext CreateNewInstance(DbContextOptions<ApplicationWriteDbContext> options)
    {
        return new ApplicationWriteDbContext(options);
    }

    protected override void ConfigureOptions(DbContextOptionsBuilder<ApplicationWriteDbContext> optionsBuilder, string connectionString)
    {
        optionsBuilder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.CommandTimeout(300);
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(10),
                errorNumbersToAdd: null);
        });
    }
}

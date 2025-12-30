using Microsoft.EntityFrameworkCore;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts.DesignTimeFactories;

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

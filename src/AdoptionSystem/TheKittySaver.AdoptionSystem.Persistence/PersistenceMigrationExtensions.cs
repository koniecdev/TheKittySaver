using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;

namespace TheKittySaver.AdoptionSystem.Persistence;

public static class PersistenceMigrationExtensions
{
    extension(IServiceProvider serviceProvider)
    {
        public async Task MigrateDatabaseAsync()
        {
            await using AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
            ApplicationWriteDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
            await dbContext.Database.MigrateAsync();
        }
    }
}

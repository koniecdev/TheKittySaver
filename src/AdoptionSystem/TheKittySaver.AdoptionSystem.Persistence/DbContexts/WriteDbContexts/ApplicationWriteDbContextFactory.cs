using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;

internal sealed class ApplicationWriteDbContextFactory : IDesignTimeDbContextFactory<ApplicationWriteDbContext>
{
    public ApplicationWriteDbContext CreateDbContext(string[] args)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        string apiProjectPath = Path.Combine(currentDirectory, "..", "TheKittySaver.AdoptionSystem.API");

        if (!Directory.Exists(apiProjectPath))
        {
            apiProjectPath = currentDirectory;
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration.GetSection("ConnectionStringSettings:Database").Value;

        if (string.IsNullOrEmpty(connectionString))
        {
            connectionString = "Server=localhost\\SQLEXPRESS;Database=TheKittySaver;Trusted_Connection=True;TrustServerCertificate=True";
        }

        DbContextOptionsBuilder<ApplicationWriteDbContext> optionsBuilder = new DbContextOptionsBuilder<ApplicationWriteDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationWriteDbContext(optionsBuilder.Options);
    }
}

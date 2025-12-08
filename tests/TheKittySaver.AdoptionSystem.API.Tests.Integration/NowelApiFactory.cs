using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration;

public class TheKittySaverApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();
    
    public string ConnectionString { get; private set; } = string.Empty;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStringSettings:Database", ConnectionString },
                });
        });

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton(this);
        });
    
        builder.ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
        });
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        ConnectionString = _msSqlContainer.GetConnectionString();

        using IServiceScope scope = Services.CreateScope();
        ApplicationWriteDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
        
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }
}

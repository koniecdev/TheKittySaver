using System.Diagnostics.CodeAnalysis;
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

#pragma warning disable CA1515
// ReSharper disable once ClassNeverInstantiated.Global
public class TheKittySaverApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
#pragma warning restore CA1515
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();

    private string _connectionString = string.Empty;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStringSettings:Database", _connectionString },
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
        _connectionString = _msSqlContainer.GetConnectionString();

        using IServiceScope scope = Services.CreateScope();
        ApplicationWriteDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
        
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }
}

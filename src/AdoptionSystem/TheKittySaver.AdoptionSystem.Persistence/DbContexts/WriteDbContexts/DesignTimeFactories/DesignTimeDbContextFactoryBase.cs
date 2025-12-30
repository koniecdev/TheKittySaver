using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts.DesignTimeFactories;

internal abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    private const string ConfigurationSection = "ConnectionStringSettings";
    private const string DefaultConnectionStringName = "Database";
    private const string DefaultEnvironment = "Local";

    protected virtual string ConnectionStringName => DefaultConnectionStringName;

    public TContext CreateDbContext(string[] args)
    {
        string? connectionStringOverride = GetArgumentValue(args, "--connection");

        if (!string.IsNullOrWhiteSpace(connectionStringOverride))
        {
            LogInfo("Using connection string from command line arguments");
            return CreateContextWithConnectionString(connectionStringOverride);
        }

        string environment = GetArgumentValue(args, "--environment")
                             ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                             ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                             ?? DefaultEnvironment;

        LogInfo($"Environment: {environment}");

        string basePath = ResolveBasePath();
        LogInfo($"Configuration base path: {basePath}");

        string connectionString = BuildConfiguration(basePath, environment);
        return CreateContextWithConnectionString(connectionString);
    }

    protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

    protected abstract void ConfigureOptions(DbContextOptionsBuilder<TContext> optionsBuilder, string connectionString);

    private TContext CreateContextWithConnectionString(string connectionString)
    {
        ValidateConnectionString(connectionString);

        DbContextOptionsBuilder<TContext> optionsBuilder = new();
        ConfigureOptions(optionsBuilder, connectionString);

        return CreateNewInstance(optionsBuilder.Options);
    }

    private string BuildConfiguration(string basePath, string environment)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration[$"{ConfigurationSection}:{ConnectionStringName}"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConfigurationSection}:{ConnectionStringName}' not found. " +
                $"Searched in: appsettings.json, appsettings.{environment}.json, appsettings.Local.json, environment variables. " +
                $"Base path: {basePath}");
        }

        return connectionString;
    }

    private static string ResolveBasePath()
    {
        string currentDir = Directory.GetCurrentDirectory();

        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            return currentDir;
        }

        DirectoryInfo? directory = new(currentDir);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        return currentDir;
    }

    private void ValidateConnectionString(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                $"Connection string '{ConnectionStringName}' is null or empty.",
                nameof(connectionString));
        }

        if (!connectionString.Contains("Server", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.Contains("Data Source", StringComparison.OrdinalIgnoreCase))
        {
            LogWarning("Connection string may be invalid - missing Server/Data Source specification");
        }
    }

    private static string? GetArgumentValue(string[] args, string argumentName)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].Equals(argumentName, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private static void LogInfo(string message)
    {
        Console.WriteLine($"[DesignTimeFactory] {message}");
    }

    private static void LogWarning(string message)
    {
        Console.WriteLine($"[DesignTimeFactory] WARNING: {message}");
    }
}

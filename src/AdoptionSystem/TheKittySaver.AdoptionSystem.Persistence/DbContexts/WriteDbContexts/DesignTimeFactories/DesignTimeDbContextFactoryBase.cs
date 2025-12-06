using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts.DesignTimeFactories;

/// <summary>
/// Base class for design-time DbContext creation used by EF Core tools.
/// Supports: dotnet ef migrations add/remove/script, dotnet ef database update
/// </summary>
/// <typeparam name="TContext">The DbContext type to create</typeparam>
internal abstract class DesignTimeDbContextFactoryBase<TContext> : IDesignTimeDbContextFactory<TContext>
    where TContext : DbContext
{
    private const string DefaultConnectionStringName = "Database";
    private const string DefaultEnvironment = "Development";
    
    /// <summary>
    /// Override to use a different connection string name from appsettings.
    /// </summary>
    protected virtual string ConnectionStringName => DefaultConnectionStringName;

    /// <summary>
    /// Creates DbContext instance for EF Core tools.
    /// </summary>
    /// <param name="args">
    /// Command line arguments. Supports:
    /// --connection "your-connection-string" to override appsettings
    /// --environment EnvironmentName to specify environment
    /// </param>
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

    /// <summary>
    /// Implement to create the DbContext instance with configured options.
    /// </summary>
    protected abstract TContext CreateNewInstance(DbContextOptions<TContext> options);

    /// <summary>
    /// Configure database provider and options.
    /// </summary>
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
            .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false) // gitignored local overrides
            .AddEnvironmentVariables()
            .Build();

        string? connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' not found. " +
                $"Searched in: appsettings.json, appsettings.{environment}.json, appsettings.Local.json, environment variables. " +
                $"Base path: {basePath}");
        }

        return connectionString;
    }

    private static string ResolveBasePath()
    {
        // EF Core tools set current directory to the project folder
        string currentDir = Directory.GetCurrentDirectory();

        if (File.Exists(Path.Combine(currentDir, "appsettings.json")))
        {
            return currentDir;
        }

        // Fallback: search parent directories (useful when running from bin/Debug/...)
        DirectoryInfo? directory = new(currentDir);
        
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "appsettings.json")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        // Last resort: use current directory and let ConfigurationBuilder fail with clear message
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

        // Basic sanity check for SQL Server connection string
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

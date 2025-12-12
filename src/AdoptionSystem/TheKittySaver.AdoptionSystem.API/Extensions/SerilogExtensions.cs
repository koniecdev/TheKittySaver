using Serilog;
using Serilog.Events;

namespace TheKittySaver.AdoptionSystem.API.Extensions;

/// <summary>
/// Extension methods for configuring Serilog logging.
/// </summary>
public static class SerilogExtensions
{
    /// <summary>
    /// Configures Serilog with bootstrap logger for startup logging.
    /// This should be called at the very beginning of Program.cs.
    /// </summary>
    public static void ConfigureBootstrapLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
    }

    /// <summary>
    /// Configures Serilog from appsettings.json configuration.
    /// </summary>
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, configuration) =>
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()
                .Enrich.WithProperty("Application", "TheKittySaver.AdoptionSystem.API")
                .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);
        });

        return builder;
    }

    /// <summary>
    /// Adds Serilog request logging middleware with custom configuration.
    /// </summary>
    public static WebApplication UseSerilogRequestLoggingMiddleware(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            // Customize the message template
            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

            // Attach additional properties to the request completion event
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
                diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);

                if (httpContext.User.Identity?.IsAuthenticated == true)
                {
                    diagnosticContext.Set("UserId", httpContext.User.Identity.Name);
                }
            };

            // Don't log health check endpoints at Information level
            options.GetLevel = (httpContext, elapsed, exception) =>
            {
                if (exception is not null)
                {
                    return LogEventLevel.Error;
                }

                if (httpContext.Response.StatusCode >= 500)
                {
                    return LogEventLevel.Error;
                }

                // Health checks and other internal endpoints
                if (httpContext.Request.Path.StartsWithSegments("/health") ||
                    httpContext.Request.Path.StartsWithSegments("/alive"))
                {
                    return LogEventLevel.Debug;
                }

                // Swagger endpoints
                if (httpContext.Request.Path.StartsWithSegments("/swagger"))
                {
                    return LogEventLevel.Debug;
                }

                return LogEventLevel.Information;
            };
        });

        return app;
    }
}

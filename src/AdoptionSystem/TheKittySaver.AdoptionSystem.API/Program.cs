using System.Reflection;
using TheKittySaver.AdoptionSystem.API;
using TheKittySaver.AdoptionSystem.API.Extensions;
using TheKittySaver.AdoptionSystem.API.Middleware;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;
using TheKittySaver.AdoptionSystem.Persistence;
using TheKittySaver.AdoptionSystem.Persistence.Settings;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.Register(builder.Configuration);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionStringFactory: sp => sp.GetRequiredService<IOptions<ConnectionStringSettings>>().Value.Database,
        name: "sqlserver",
        tags: ["db", "sql", "ready"]);

WebApplication app = builder.Build();

await app.Services.MigrateDatabaseAsync();

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        if (httpContext.Items.TryGetValue("CorrelationId", out object? correlationId))
        {
            diagnosticContext.Set("CorrelationId", correlationId);
        }
    };
});

app.UseExceptionHandler();

if (app.Environment.IsEnvironment("Local"))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();
RouteGroupBuilder versionedGroup = app
    .MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(apiVersionSet);

app.MapEndpoints(versionedGroup);

await app.RunAsync();

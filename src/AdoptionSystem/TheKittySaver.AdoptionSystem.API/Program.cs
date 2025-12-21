using System.Globalization;
using System.Reflection;
using System.Threading.RateLimiting;
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

const string localCorsPolicy = "DevelopmentPolicy";
const string productionCorsPolicy = "ProductionPolicy";

builder.Services.AddCors(options =>
{
    options.AddPolicy(localCorsPolicy, corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
    options.AddPolicy(productionCorsPolicy, corsBuilder =>
    {
        corsBuilder.WithOrigins(
            "https://uratujkota.koniec.dev",
            "https://uratujkota.pl",
            "https://auth.uratujkota.pl"
        ).AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services
    .AddHealthChecks()
    .AddSqlServer(
        connectionStringFactory: sp => sp.GetRequiredService<IOptions<ConnectionStringSettings>>().Value.Database,
        name: "sqlserver",
        tags: ["db", "sql", "ready"]);

const string rateLimitPolicy = "fixed-by-ip";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy(rateLimitPolicy, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

WebApplication app = builder.Build();

const string localEnvironment = "local";
const string productionEnvironment = "production";

if (app.Environment.IsEnvironment(localEnvironment))
{
    await app.Services.MigrateDatabaseAsync();
}

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
app.UseHsts();
app.UseHttpsRedirection();
app.UseRouting();
string corsPolicy = app.Environment.EnvironmentName.ToLower(CultureInfo.InvariantCulture) switch
{
    productionEnvironment => productionCorsPolicy, 
    _ => localCorsPolicy
};
app.UseCors(corsPolicy);

app.UseRateLimiter();

if (app.Environment.IsEnvironment(localEnvironment))
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

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
    .WithApiVersionSet(apiVersionSet)
    .RequireRateLimiting(rateLimitPolicy);

app.MapEndpoints(versionedGroup);

await app.RunAsync();

using System.Reflection;
using TheKittySaver.AdoptionSystem.API;
using TheKittySaver.AdoptionSystem.API.Extensions;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Serilog;

// Configure bootstrap logger for startup logging
SerilogExtensions.ConfigureBootstrapLogger();

try
{
    Log.Information("Application is starting up");

    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    // Serilog configuration from appsettings.json
    builder.AddSerilog();

    // Core services
    builder.Services.Register(builder.Configuration);
    builder.Services.AddEndpointsApiExplorer();

    // OpenAPI / Swagger configuration
    builder.Services.AddOpenApi();
    builder.Services.AddSwaggerConfiguration();

    // Exception handling
    builder.Services.AddExceptionHandlers();

    // CORS configuration
    builder.Services.AddCorsConfiguration(builder.Configuration);

    // API versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ReportApiVersions = true;
        options.AssumeDefaultVersionWhenUnspecified = true;
    }).AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

    builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

    WebApplication app = builder.Build();

    // Exception handling middleware - must be first
    app.UseExceptionHandler();

    // Serilog request logging
    app.UseSerilogRequestLoggingMiddleware();

    // Security headers and HTTPS redirection
    app.UseHttpsRedirection();

    // CORS middleware
    app.UseCorsConfiguration();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // API versioning
    ApiVersionSet apiVersionSet = app.NewApiVersionSet()
        .HasApiVersion(new ApiVersion(1))
        .ReportApiVersions()
        .Build();

    RouteGroupBuilder versionedGroup = app
        .MapGroup("api/v{apiVersion:apiVersion}")
        .WithApiVersionSet(apiVersionSet);

    // Map all endpoints
    app.MapEndpoints(versionedGroup);

    // Development-only middleware
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerConfiguration();
    }

    Log.Information("Application started successfully");

    await app.RunAsync();
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
}
finally
{
    Log.Information("Application is shutting down");
    await Log.CloseAndFlushAsync();
}

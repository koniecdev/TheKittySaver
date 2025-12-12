namespace TheKittySaver.AdoptionSystem.API.Extensions;

/// <summary>
/// Extension methods for configuring CORS policies.
/// </summary>
public static class CorsExtensions
{
    public const string DevelopmentPolicyName = "DevelopmentPolicy";
    public const string ProductionPolicyName = "ProductionPolicy";

    /// <summary>
    /// Adds CORS policies for development and production environments.
    /// </summary>
    public static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsSection = configuration.GetSection("Cors:AllowedOrigins");
        var productionOrigins = corsSection.GetSection("Production").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            // Development policy - allows everything
            options.AddPolicy(DevelopmentPolicyName, builder =>
            {
                builder
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });

            // Production policy - restricted origins
            options.AddPolicy(ProductionPolicyName, builder =>
            {
                if (productionOrigins.Length > 0)
                {
                    builder
                        .WithOrigins(productionOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
                }
                else
                {
                    // Fallback: no CORS allowed if not configured
                    builder
                        .SetIsOriginAllowed(_ => false)
                        .DisallowCredentials();
                }
            });
        });

        return services;
    }

    /// <summary>
    /// Adds CORS middleware with environment-appropriate policy.
    /// </summary>
    public static WebApplication UseCorsConfiguration(this WebApplication app)
    {
        var policyName = app.Environment.IsDevelopment()
            ? DevelopmentPolicyName
            : ProductionPolicyName;

        app.UseCors(policyName);

        return app;
    }
}

using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace TheKittySaver.AdoptionSystem.API.Extensions;

internal static class HttpClientResilienceExtensions
{
    public static IHttpClientBuilder AddStandardResiliencePolicy(this IHttpClientBuilder builder)
    {
        builder.AddResilienceHandler("StandardResilience", pipeline =>
        {
            pipeline.AddConcurrencyLimiter(100, 50);

            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(500),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result?.IsSuccessStatusCode == false ||
                    args.Outcome.Exception is not null)
            });

            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                SamplingDuration = TimeSpan.FromSeconds(30),
                FailureRatio = 0.5,
                MinimumThroughput = 10,
                BreakDuration = TimeSpan.FromSeconds(30),
                ShouldHandle = args => ValueTask.FromResult(
                    args.Outcome.Result?.IsSuccessStatusCode == false ||
                    args.Outcome.Exception is not null)
            });

            pipeline.AddTimeout(TimeSpan.FromSeconds(10));
        });

        return builder;
    }

    public static IServiceCollection AddResilientHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        string baseAddress)
        where TClient : class
        where TImplementation : class, TClient
    {
        services.AddHttpClient<TClient, TImplementation>(client =>
            {
                client.BaseAddress = new Uri(baseAddress);
            })
            .AddStandardResiliencePolicy();

        return services;
    }
}

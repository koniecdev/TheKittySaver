using System.Text.Json;
using Bogus;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

//Test classes must be public.
#pragma warning disable CA1515
public abstract class AsyncLifetimeTestBase : IAsyncLifetime
#pragma warning restore CA1515
{
    protected Faker Faker { get; } = new();
    protected abstract HttpClient HttpClient { get; }
    protected abstract JsonSerializerOptions JsonSerializerOptions { get; }

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanerService.CleanDatabaseAsync(HttpClient, JsonSerializerOptions);
}

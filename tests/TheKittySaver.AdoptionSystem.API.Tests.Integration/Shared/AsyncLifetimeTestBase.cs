using Bogus;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

internal abstract class AsyncLifetimeTestBase : IAsyncLifetime
{
    protected Faker Faker { get; } = new();
    protected abstract TestApiClient ApiClient { get; }

    public virtual Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await CleanerService.CleanDatabaseAsync(ApiClient);
}

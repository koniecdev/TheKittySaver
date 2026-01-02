// Shared/PersonEndpointsTestBase.cs

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

[Collection("Api")]
public abstract class PersonEndpointsTestBase : AsyncLifetimeTestBase
{
    protected override TestApiClient ApiClient { get; }

    protected PersonEndpointsTestBase(TheKittySaverApiFactory appFactory)
    {
        JsonSerializerOptions jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptionsSnapshot<JsonOptions>>().Value.SerializerOptions;

        ApiClient = new TestApiClient(appFactory.CreateClient(), jsonSerializerOptions);
    }
}

using System.Text.Json;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

internal sealed class TestApiClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
{
    public HttpClient Http { get; } = httpClient;
    public JsonSerializerOptions JsonOptions { get; } = jsonSerializerOptions;
}

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

public sealed class TestApiClient
{
    public TestApiClient(HttpClient httpClient, JsonSerializerOptions jsonSerializerOptions)
    {
        Http = httpClient;
        JsonOptions = jsonSerializerOptions;
    }

    public HttpClient Http { get; }
    public JsonSerializerOptions JsonOptions { get; }
}

using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class PersonApiFactory
{
    public static async Task<PersonResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker)
    {
        CreatePersonRequest request = CreateRandomRequest(faker);

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/v1/persons", request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        PersonResponse personResponse = JsonSerializer.Deserialize<PersonResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");
        return personResponse;
    }

    public static CreatePersonRequest CreateRandomRequest(Faker faker)
    {
        CreatePersonRequest request = new(
            IdentityId.New(),
            faker.Internet.UserName(),
            faker.Internet.Email(),
            faker.Person.PolishPhoneNumber());
        return request;
    }
}

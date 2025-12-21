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
    public static CreatePersonRequest GenerateRandomCreateRequest(Faker faker)
    {
        CreatePersonRequest request = new(
            IdentityId.New(),
            faker.Internet.UserName(),
            faker.Internet.Email(),
            faker.Person.PolishPhoneNumber());
        return request;
    }
    
    public static UpdatePersonRequest GenerateRandomUpdateRequest(Faker faker)
    {
        UpdatePersonRequest request = new(
            faker.Internet.UserName(),
            faker.Internet.Email(),
            faker.Person.PolishPhoneNumber());
        return request;
    }
    
    public static async Task<PersonId> CreateRandomAndGetIdAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker)
    {
        CreatePersonRequest request = GenerateRandomCreateRequest(faker);

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(new Uri("api/v1/persons", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        PersonId personId = JsonSerializer.Deserialize<PersonId>(stringResponse, jsonSerializerOptions);
        return personId;
    }
    
    public static async Task<PersonDetailsResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker)
    {
        PersonId personId = await CreateRandomAndGetIdAsync(httpClient, jsonSerializerOptions, faker);

        PersonDetailsResponse personResponse = await GetAsync(httpClient, jsonSerializerOptions, personId);
        return personResponse;
    }

    public static async Task<PersonDetailsResponse> GetAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        PersonId personId)
    {
        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(new Uri($"api/v1/persons/{personId.Value}", UriKind.Relative));
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        PersonDetailsResponse personResponse = JsonSerializer.Deserialize<PersonDetailsResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonDetailsResponse");
        return personResponse;
    }
}

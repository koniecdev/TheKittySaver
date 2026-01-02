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
    public static CreatePersonRequest GenerateRandomCreateRequest(Faker faker) => new(
        IdentityId.New(),
        faker.Internet.UserName(),
        faker.Internet.Email(),
        faker.Person.PolishPhoneNumber());

    public static UpdatePersonRequest GenerateRandomUpdateRequest(Faker faker) => new(
        faker.Internet.UserName(),
        faker.Internet.Email(),
        faker.Person.PolishPhoneNumber());

    public static async Task<PersonId> CreateAndGetIdAsync(TestApiClient apiClient, CreatePersonRequest request)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PostAsJsonAsync(new Uri("api/v1/persons", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<PersonId>(stringResponse, apiClient.JsonOptions);
    }
    
    public static async Task<PersonId> CreateRandomAndGetIdAsync(TestApiClient apiClient, Faker faker)
    {
        CreatePersonRequest request = GenerateRandomCreateRequest(faker);
        return await CreateAndGetIdAsync(apiClient, request);
    }

    public static async Task<PersonDetailsResponse> CreateRandomAsync(TestApiClient apiClient, Faker faker)
    {
        PersonId personId = await CreateRandomAndGetIdAsync(apiClient, faker);
        return await PersonApiQueryService.GetByIdAsync(apiClient, personId);
    }
}

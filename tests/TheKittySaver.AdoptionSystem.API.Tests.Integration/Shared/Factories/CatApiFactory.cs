using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class CatApiFactory
{
    public static async Task<CatResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker,
        PersonId personId,
        string? name = null)
    {
        CreateCatRequest request = CreateRandomRequest(faker, personId, name);

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/v1/cats", request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");
        return catResponse;
    }

    public static async Task<CatResponse> GetAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        CatId catId)
    {
        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync($"api/v1/cats/{catId.Value}");
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");
        return catResponse;
    }

    public static CreateCatRequest CreateRandomRequest(Faker faker, PersonId personId, string? name = null)
    {
        CreateCatRequest request = new(
            personId,
            name ?? faker.Name.FirstName(),
            faker.Lorem.Sentence(),
            faker.Random.Int(1, 15),
            faker.PickRandom<CatGenderType>(),
            faker.PickRandom<ColorType>(),
            faker.Random.Decimal(2.0m, 8.0m),
            HealthStatusType.Healthy,
            false,
            null,
            SpecialNeedsSeverityType.None,
            faker.PickRandom<TemperamentType>(),
            0,
            null,
            null,
            ListingSourceType.Shelter,
            faker.Company.CompanyName(),
            true,
            FivStatus.Negative,
            FelvStatus.Negative,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));
        return request;
    }

    public static CreateCatRequest CreateFixedRequest(PersonId personId, string? name = null)
    {
        CreateCatRequest request = new(
            personId,
            name ?? "Mruczek",
            "Friendly orange cat",
            3,
            CatGenderType.Male,
            ColorType.Orange,
            4.5m,
            HealthStatusType.Healthy,
            false,
            null,
            SpecialNeedsSeverityType.None,
            TemperamentType.Friendly,
            0,
            null,
            null,
            ListingSourceType.Shelter,
            "Test Shelter",
            true,
            FivStatus.Negative,
            FelvStatus.Negative,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));
        return request;
    }

    public static async Task<CatResponse> CreateHealthyCatWithThumbnail(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        PersonId personId,
        string? name = null)
    {
        CreateCatRequest request = CreateFixedRequest(personId, name);
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync("api/v1/cats", request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, jsonSerializerOptions)
                                  ?? throw new JsonException("Failed to deserialize CatResponse");
        
        
    }
}

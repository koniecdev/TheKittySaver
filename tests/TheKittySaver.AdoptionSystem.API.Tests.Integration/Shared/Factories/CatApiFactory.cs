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
    public static async Task<CatDetailsResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker,
        PersonId personId,
        string? name = null)
    {
        CreateCatRequest request = CreateRandomRequest(faker, personId, name);

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(new Uri("api/v1/cats", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatId catId = JsonSerializer.Deserialize<CatId>(stringResponse, jsonSerializerOptions);

        CatDetailsResponse catResponse = await GetAsync(httpClient, jsonSerializerOptions, catId);
        return catResponse;
    }

    public static async Task<CatDetailsResponse> GetAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        CatId catId)
    {
        HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(new Uri($"api/v1/cats/{catId.Value}", UriKind.Relative));
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        CatDetailsResponse catResponse = JsonSerializer.Deserialize<CatDetailsResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatDetailsResponse");
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

    public static async Task<CatDetailsResponse> CreateHealthyCatWithThumbnail(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        PersonId personId,
        string? name = null)
    {
        CreateCatRequest request = CreateFixedRequest(personId, name);
        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(new Uri("api/v1/cats", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatId catId = JsonSerializer.Deserialize<CatId>(stringResponse, jsonSerializerOptions);

        await CatGalleryApiFactory.CreateThumbnailAsync(httpClient, jsonSerializerOptions, catId);

        CatDetailsResponse updatedCatResponse = await GetAsync(httpClient, jsonSerializerOptions, catId);
        return updatedCatResponse;
    }
}

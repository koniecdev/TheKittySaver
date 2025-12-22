using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class CatApiFactory
{
    public static CreateCatRequest GenerateRandomCreateRequest(Faker faker, PersonId personId) => new(
        PersonId: personId,
        Name: faker.Name.FirstName(),
        Description: faker.Lorem.Sentence(),
        Age: faker.Random.Int(CatAge.MinimumAllowedValue, CatAge.MaximumAllowedValue),
        Gender: faker.PickRandom<CatGenderType>(),
        Color: faker.PickRandom<ColorType>(),
        WeightValueInKilograms: faker.Random.Decimal(CatWeight.MinWeightKg, 10),
        HealthStatus: faker.PickRandom<HealthStatusType>(),
        SpecialNeedsStatusHasSpecialNeeds: false,
        SpecialNeedsStatusDescription: null,
        SpecialNeedsStatusSeverityType: SpecialNeedsSeverityType.None,
        Temperament: faker.PickRandom<TemperamentType>(),
        AdoptionHistoryReturnCount: 0,
        AdoptionHistoryLastReturnDate: null,
        AdoptionHistoryLastReturnReason: null,
        ListingSourceType: ListingSourceType.PrivatePerson,
        ListingSourceSourceName: faker.Company.CompanyName(),
        IsNeutered: faker.Random.Bool(),
        InfectiousDiseaseStatusFivStatus: FivStatus.Negative,
        InfectiousDiseaseStatusFelvStatus: FelvStatus.Negative,
        InfectiousDiseaseStatusLastTestedAt: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)));

    public static UpdateCatRequest GenerateRandomUpdateRequest(Faker faker) => new(
        Name: faker.Name.FirstName(),
        Description: faker.Lorem.Sentence(),
        Age: faker.Random.Int(1, 20),
        Gender: faker.PickRandom<CatGenderType>(),
        Color: faker.PickRandom<ColorType>(),
        WeightValueInKilograms: faker.Random.Decimal(2, 10),
        HealthStatus: faker.PickRandom<HealthStatusType>(),
        HasSpecialNeeds: false,
        SpecialNeedsDescription: null,
        SpecialNeedsSeverityType: SpecialNeedsSeverityType.None,
        Temperament: faker.PickRandom<TemperamentType>(),
        AdoptionHistoryReturnCount: 0,
        AdoptionHistoryLastReturnDate: null,
        AdoptionHistoryLastReturnReason: null,
        ListingSourceType: ListingSourceType.PrivatePerson,
        ListingSourceSourceName: faker.Company.CompanyName(),
        IsNeutered: faker.Random.Bool(),
        FivStatus: FivStatus.Negative,
        FelvStatus: FelvStatus.Negative,
        InfectiousDiseaseStatusLastTestedAt: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30)));

    public static async Task<CatId> CreateRandomAndGetIdAsync(TestApiClient apiClient, Faker faker, PersonId personId)
    {
        CreateCatRequest request = GenerateRandomCreateRequest(faker, personId);

        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PostAsJsonAsync(new Uri("api/v1/cats", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<CatId>(stringResponse, apiClient.JsonOptions);
    }

    public static async Task<CatDetailsResponse> CreateRandomAsync(TestApiClient apiClient, Faker faker, PersonId personId)
    {
        CatId catId = await CreateRandomAndGetIdAsync(apiClient, faker, personId);
        return await CatApiQueryService.GetByIdAsync(apiClient, catId);
    }

    public static async Task<CatId> CreateRandomWithThumbnailAndGetIdAsync(TestApiClient apiClient, Faker faker, PersonId personId)
    {
        CatId catId = await CreateRandomAndGetIdAsync(apiClient, faker, personId);
        _ = await CatGalleryApiFactory.UpsertThumbnailAsync(apiClient, catId);
        return catId;
    }
}

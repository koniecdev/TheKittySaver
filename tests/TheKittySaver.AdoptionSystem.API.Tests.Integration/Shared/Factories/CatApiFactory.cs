using Bogus;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class CatApiFactory
{
    public static CreateCatRequest GenerateRandomCreateRequest(
        Faker faker,
        PersonId personId,
        bool isHealthy = false)
    {
        bool hasSpecialNeeds = faker.PickRandom(true, false);
        string? description = hasSpecialNeeds
            ? faker.Lorem.Sentence()
            : null;
        SpecialNeedsSeverityType specialNeedsSeverityType = hasSpecialNeeds
            ? faker.PickRandomWithout(SpecialNeedsSeverityType.Unset)
            : SpecialNeedsSeverityType.None;

        int returnCount = faker.Random.Int(0, 3);
        string? lastReturnReason = returnCount > 0 ? faker.Lorem.Sentence() : null;
        DateTimeOffset? lastReturnDate = returnCount > 0 ? faker.Date.PastOffset(returnCount) : null;

        FivStatus infectiousDiseaseStatusFivStatus;
        FelvStatus infectiousDiseaseStatusFelvStatus;

        if (isHealthy)
        {
            infectiousDiseaseStatusFivStatus = FivStatus.Negative;
            infectiousDiseaseStatusFelvStatus = FelvStatus.Negative;
        }
        else
        {
            bool fivPositive = faker.Random.Bool();
            bool felvPositive = !fivPositive || faker.Random.Bool();

            infectiousDiseaseStatusFivStatus = fivPositive ? FivStatus.Positive : FivStatus.Negative;
            infectiousDiseaseStatusFelvStatus = felvPositive ? FelvStatus.Positive : FelvStatus.Negative;
        }

        DateOnly? infectiousDiseaseStatusLastTestedAt =
            DateOnly.FromDateTime(faker.Date.PastOffset().DateTime);

        return new CreateCatRequest(
            PersonId: personId.Value,
            Name: faker.Name.FirstName(),
            Description: faker.Lorem.Sentence(),
            Age: faker.Random.Int(CatAge.MinimumAllowedValue, CatAge.MaximumAllowedValue),
            Gender: faker.PickRandomWithout(CatGenderType.Unset),
            Color: faker.PickRandomWithout(ColorType.Unset),
            WeightInGrams: faker.Random.Int(CatWeight.MinWeightGrams, CatWeight.MaxWeightGrams),
            HealthStatus: faker.PickRandomWithout(HealthStatusType.Unset),
            HasSpecialNeeds: hasSpecialNeeds,
            SpecialNeedsDescription: description,
            SpecialNeedsSeverityType: specialNeedsSeverityType,
            Temperament: faker.PickRandomWithout(TemperamentType.Unset),
            AdoptionHistoryReturnCount: returnCount,
            AdoptionHistoryLastReturnDate: lastReturnDate,
            AdoptionHistoryLastReturnReason: lastReturnReason,
            IsNeutered: faker.Random.Bool(),
            FivStatus: infectiousDiseaseStatusFivStatus,
            FelvStatus: infectiousDiseaseStatusFelvStatus,
            InfectiousDiseaseStatusLastTestedAt: infectiousDiseaseStatusLastTestedAt);
    }

    public static UpdateCatRequest GenerateRandomUpdateRequest(Faker faker, bool isHealthy = false)
    {
        bool hasSpecialNeeds = faker.PickRandom(true, false);
        string? description = hasSpecialNeeds
            ? faker.Lorem.Sentence()
            : null;
        SpecialNeedsSeverityType specialNeedsSeverityType = hasSpecialNeeds
            ? faker.PickRandomWithout(SpecialNeedsSeverityType.Unset)
            : SpecialNeedsSeverityType.None;

        int returnCount = faker.Random.Int(0, 3);
        string? lastReturnReason = returnCount > 0 ? faker.Lorem.Sentence() : null;
        DateTimeOffset? lastReturnDate = returnCount > 0 ? faker.Date.PastOffset(returnCount) : null;

        FivStatus infectiousDiseaseStatusFivStatus;
        FelvStatus infectiousDiseaseStatusFelvStatus;

        if (isHealthy)
        {
            infectiousDiseaseStatusFivStatus = FivStatus.Negative;
            infectiousDiseaseStatusFelvStatus = FelvStatus.Negative;
        }
        else
        {
            // Ensure at least one is positive when not healthy
            bool fivPositive = faker.Random.Bool();
            bool felvPositive = !fivPositive || faker.Random.Bool();

            infectiousDiseaseStatusFivStatus = fivPositive ? FivStatus.Positive : FivStatus.Negative;
            infectiousDiseaseStatusFelvStatus = felvPositive ? FelvStatus.Positive : FelvStatus.Negative;
        }

        DateOnly? infectiousDiseaseStatusLastTestedAt =
            DateOnly.FromDateTime(faker.Date.PastOffset().DateTime);

        return new UpdateCatRequest(
            Name: faker.Name.FirstName(),
            Description: faker.Lorem.Sentence(),
            Age: faker.Random.Int(CatAge.MinimumAllowedValue, CatAge.MaximumAllowedValue),
            Gender: faker.PickRandomWithout(CatGenderType.Unset),
            Color: faker.PickRandomWithout(ColorType.Unset),
            WeightInGrams: faker.Random.Int(CatWeight.MinWeightGrams, CatWeight.MaxWeightGrams),
            HealthStatus: faker.PickRandomWithout(HealthStatusType.Unset),
            HasSpecialNeeds: hasSpecialNeeds,
            SpecialNeedsDescription: description,
            SpecialNeedsSeverityType: specialNeedsSeverityType,
            Temperament: faker.PickRandomWithout(TemperamentType.Unset),
            AdoptionHistoryReturnCount: returnCount,
            AdoptionHistoryLastReturnDate: lastReturnDate,
            AdoptionHistoryLastReturnReason: lastReturnReason,
            IsNeutered: faker.Random.Bool(),
            FivStatus: infectiousDiseaseStatusFivStatus,
            FelvStatus: infectiousDiseaseStatusFelvStatus,
            InfectiousDiseaseStatusLastTestedAt: infectiousDiseaseStatusLastTestedAt);
    }

    public static async Task<CatId> CreateAndGetIdAsync(TestApiClient apiClient, CreateCatRequest request)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PostAsJsonAsync(new Uri("api/v1/cats", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<CatId>(stringResponse, apiClient.JsonOptions);
    }
    
    public static async Task<CatId> CreateRandomAndGetIdAsync(
        TestApiClient apiClient,
        Faker faker,
        PersonId personId,
        bool isHealthy = false)
    {
        CreateCatRequest request = GenerateRandomCreateRequest(faker, personId, isHealthy);
        return await CreateAndGetIdAsync(apiClient, request);
    }

    public static async Task<CatDetailsResponse> CreateRandomAsync(
        TestApiClient apiClient,
        Faker faker,
        PersonId personId,
        bool isHealthy = false)
    {
        CatId catId = await CreateRandomAndGetIdAsync(apiClient, faker, personId, isHealthy);
        return await CatApiQueryService.GetByIdAsync(apiClient, catId);
    }
}

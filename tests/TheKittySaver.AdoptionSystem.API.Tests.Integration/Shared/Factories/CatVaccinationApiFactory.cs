using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class CatVaccinationApiFactory
{
    public static async Task<CatVaccinationResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker,
        CatId catId,
        VaccinationType type = VaccinationType.Rabies)
    {
        CreateCatVaccinationRequest request = CreateRandomRequest(faker, type);

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(
            new Uri($"api/v1/cats/{catId.Value}/vaccinations", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        CatVaccinationResponse vaccinationResponse = JsonSerializer.Deserialize<CatVaccinationResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatVaccinationResponse");
        return vaccinationResponse;
    }

    public static CreateCatVaccinationRequest CreateRandomRequest(Faker faker, VaccinationType type = VaccinationType.Rabies)
    {
        CreateCatVaccinationRequest request = new(
            type,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            faker.Lorem.Sentence());
        return request;
    }
}

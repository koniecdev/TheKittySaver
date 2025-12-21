using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class AdoptionAnnouncementApiFactory
{
    public static CreateAdoptionAnnouncementRequest GenerateRandomCreateRequest(Faker faker, CatId catId) => new(
        CatId: catId,
        Description: faker.Lorem.Sentence(),
        AddressCountryCode: CountryCode.PL,
        AddressPostalCode: "00-001",
        AddressRegion: "Mazowieckie",
        AddressCity: faker.Address.City(),
        AddressLine: faker.Address.StreetAddress(),
        Email: faker.Internet.Email(),
        PhoneNumber: faker.Person.PolishPhoneNumber());

    public static UpdateAdoptionAnnouncementRequest GenerateRandomUpdateRequest(Faker faker) => new(
        Description: faker.Lorem.Sentence(),
        AddressCountryCode: CountryCode.PL,
        AddressPostalCode: "00-002",
        AddressRegion: "Mazowieckie",
        AddressCity: faker.Address.City(),
        AddressLine: faker.Address.StreetAddress(),
        Email: faker.Internet.Email(),
        PhoneNumber: faker.Person.PolishPhoneNumber());

    public static async Task<AdoptionAnnouncementId> CreateRandomAndGetIdAsync(
        TestApiClient apiClient,
        Faker faker,
        CatId catId)
    {
        CreateAdoptionAnnouncementRequest request = GenerateRandomCreateRequest(faker, catId);

        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PostAsJsonAsync(new Uri("api/v1/adoption-announcements", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<AdoptionAnnouncementId>(stringResponse, apiClient.JsonOptions);
    }

    public static async Task<AdoptionAnnouncementDetailsResponse> CreateRandomAsync(
        TestApiClient apiClient,
        Faker faker,
        CatId catId)
    {
        AdoptionAnnouncementId announcementId = await CreateRandomAndGetIdAsync(apiClient, faker, catId);
        return await AdoptionAnnouncementApiQueryService.GetByIdAsync(apiClient, announcementId);
    }
}

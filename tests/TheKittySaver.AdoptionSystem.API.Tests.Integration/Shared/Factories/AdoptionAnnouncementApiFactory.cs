using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class AdoptionAnnouncementApiFactory
{
    private static readonly List<(string ZipCode, string Voivodeship)> PolishAddressData =
    [
        ("89-240", "Kujawsko-Pomorskie"),
        ("00-001", "Mazowieckie"),
        ("60-365", "Wielkopolskie"),
        ("30-001", "Małopolskie"),
        ("80-001", "Pomorskie"),
        ("10-001", "Warmińsko-Mazurskie")
    ];

    public static async Task<AdoptionAnnouncementListItemResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker,
        CatId catId)
    {
        CreateAdoptionAnnouncementRequest request = CreateRandomRequest(faker, catId);

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(
            new Uri("api/v1/adoption-announcements", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        AdoptionAnnouncementListItemResponse announcementListItemResponse = JsonSerializer.Deserialize<AdoptionAnnouncementListItemResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize AdoptionAnnouncementResponse");
        return announcementListItemResponse;
    }

    public static CreateAdoptionAnnouncementRequest CreateRandomRequest(Faker faker, CatId catId)
    {
        int randomIndex = faker.Random.Int(0, PolishAddressData.Count - 1);
        (string zipCode, string voivodeship) = PolishAddressData[randomIndex];

        CreateAdoptionAnnouncementRequest request = new(
            catId,
            faker.Lorem.Sentence(10),
            CountryCode.PL,
            zipCode,
            voivodeship,
            faker.Address.City(),
            faker.Address.StreetAddress(),
            faker.Internet.Email(),
            "+48600700800");
        return request;
    }
}

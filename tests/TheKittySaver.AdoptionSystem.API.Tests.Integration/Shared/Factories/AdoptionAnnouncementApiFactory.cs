using Bogus;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class AdoptionAnnouncementApiFactory
{
    public static CreateAdoptionAnnouncementRequest GenerateRandomCreateRequest(Faker faker, IEnumerable<Guid> catIds)
    {
        PolishAddressData addressData = faker.PolishAddress();

        return new CreateAdoptionAnnouncementRequest(
            CatIds: catIds.ToList(),
            Description: faker.Lorem.Sentence(),
            AddressCountryCode: CountryCode.PL,
            AddressPostalCode: addressData.PostalCode,
            AddressRegion: addressData.Region,
            AddressCity: addressData.City,
            AddressLine: faker.Address.StreetAddress(),
            Email: faker.Internet.Email(),
            PhoneNumber: faker.Person.PolishPhoneNumber());
    }

    public static UpdateAdoptionAnnouncementRequest GenerateRandomUpdateRequest(Faker faker)
    {
        PolishAddressData addressData = faker.PolishAddress();

        return new UpdateAdoptionAnnouncementRequest(
            Description: faker.Lorem.Sentence(),
            AddressCountryCode: CountryCode.PL,
            AddressPostalCode: addressData.PostalCode,
            AddressRegion: addressData.Region,
            AddressCity: addressData.City,
            AddressLine: faker.Address.StreetAddress(),
            Email: faker.Internet.Email(),
            PhoneNumber: faker.Person.PolishPhoneNumber());
    }

    public static async Task<AdoptionAnnouncementId> CreateAndGetIdAsync(
        TestApiClient apiClient,
        CreateAdoptionAnnouncementRequest request)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PostAsJsonAsync(new Uri("api/v1/adoption-announcements", UriKind.Relative), request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<AdoptionAnnouncementId>(stringResponse, apiClient.JsonOptions);
    }

    public static async Task<AdoptionAnnouncementId> CreateRandomAndGetIdAsync(
        TestApiClient apiClient,
        Faker faker,
        IEnumerable<Guid> catIds)
    {
        CreateAdoptionAnnouncementRequest request = GenerateRandomCreateRequest(faker, catIds);
        return await CreateAndGetIdAsync(apiClient, request);
    }

    public static async Task<AdoptionAnnouncementDetailsResponse> CreateRandomAsync(
        TestApiClient apiClient,
        Faker faker,
        IEnumerable<Guid> catIds)
    {
        AdoptionAnnouncementId announcementId = await CreateRandomAndGetIdAsync(apiClient, faker, catIds);
        return await AdoptionAnnouncementApiQueryService.GetByIdAsync(apiClient, announcementId);
    }
}

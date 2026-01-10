using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Persons;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;

internal static class PersonAddressApiFactory
{
    public static CreatePersonAddressRequest GenerateRandomCreateRequest(Faker faker)
    {
        PolishAddressData addressData = faker.PolishAddress();

        return new CreatePersonAddressRequest(
            TwoLetterIsoCountryCode: CountryCode.PL,
            Name: faker.Lorem.Word(),
            PostalCode: addressData.PostalCode,
            Region: addressData.Region,
            City: addressData.City,
            Line: faker.Address.StreetAddress());
    }

    public static UpdatePersonAddressRequest GenerateRandomUpdateRequest(Faker faker)
    {
        PolishAddressData addressData = faker.PolishAddress();

        return new UpdatePersonAddressRequest(
            Name: faker.Lorem.Word(),
            PostalCode: addressData.PostalCode,
            Region: addressData.Region,
            City: addressData.City,
            Line: faker.Address.StreetAddress());
    }

    public static async Task<AddressId> CreateAndGetIdAsync(
        TestApiClient apiClient,
        PersonId personId,
        CreatePersonAddressRequest request)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PostAsJsonAsync(
                new Uri($"api/v1/persons/{personId.Value}/addresses", UriKind.Relative),
                request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<AddressId>(stringResponse, apiClient.JsonOptions);
    }

    public static async Task<AddressId> CreateRandomAndGetIdAsync(
        TestApiClient apiClient,
        Faker faker,
        PersonId personId)
    {
        CreatePersonAddressRequest request = GenerateRandomCreateRequest(faker);
        return await CreateAndGetIdAsync(apiClient, personId, request);
    }

    public static async Task<PersonAddressResponse> CreateRandomAsync(
        TestApiClient apiClient,
        Faker faker,
        PersonId personId)
    {
        AddressId addressId = await CreateRandomAndGetIdAsync(apiClient, faker, personId);
        return await PersonAddressApiQueryService.GetByIdAsync(apiClient, personId, addressId);
    }
}

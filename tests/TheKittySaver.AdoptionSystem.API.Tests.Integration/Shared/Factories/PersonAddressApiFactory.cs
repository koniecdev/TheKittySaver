using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class PersonAddressApiFactory
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

    public static async Task<PersonAddressResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        Faker faker,
        PersonId personId,
        string? name = null)
    {
        CreatePersonAddressRequest request = CreateRandomRequest(faker, name);

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsJsonAsync(
            $"api/v1/persons/{personId.Value}/addresses", request);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        PersonAddressResponse addressResponse = JsonSerializer.Deserialize<PersonAddressResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonAddressResponse");
        return addressResponse;
    }

    public static CreatePersonAddressRequest CreateRandomRequest(Faker faker, string? name = null)
    {
        int randomIndex = faker.Random.Int(0, PolishAddressData.Count - 1);
        (string zipCode, string voivodeship) = PolishAddressData[randomIndex];

        CreatePersonAddressRequest request = new(
            CountryCode.PL,
            name ?? faker.Lorem.Word(),
            zipCode,
            voivodeship,
            faker.Address.City(),
            faker.Address.StreetAddress());
        return request;
    }
}

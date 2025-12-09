using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

[Collection("Api")]
public sealed class PersonAddressEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public PersonAddressEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreatePersonAddress_ShouldReturnAddress_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CreatePersonAddressRequest request = new(
            CountryCode.PL,
            "Home",
            "00-001",
            "Mazowieckie",
            "Warszawa",
            "ul. Testowa 1");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/persons/{person.Id.Value}/addresses", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        PersonAddressResponse addressResponse = JsonSerializer.Deserialize<PersonAddressResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonAddressResponse");

        addressResponse.ShouldNotBeNull();
        addressResponse.PersonId.ShouldBe(person.Id);
        addressResponse.CountryCode.ShouldBe(CountryCode.PL);
        addressResponse.Name.ShouldBe("Home");
        addressResponse.PostalCode.ShouldBe("00-001");
        addressResponse.Region.ShouldBe("Mazowieckie");
        addressResponse.City.ShouldBe("Warszawa");
        addressResponse.Line.ShouldBe("ul. Testowa 1");
    }

    [Fact]
    public async Task CreatePersonAddress_ShouldReturnAddress_WhenLineIsNull()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CreatePersonAddressRequest request = new(
            CountryCode.PL,
            _faker.Lorem.Word(),
            _faker.Address.ZipCode("##-###"),
            _faker.Address.State(),
            _faker.Address.City());

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/persons/{person.Id.Value}/addresses", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        PersonAddressResponse addressResponse = JsonSerializer.Deserialize<PersonAddressResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonAddressResponse");

        addressResponse.ShouldNotBeNull();
        addressResponse.Line.ShouldBeNull();
    }

    [Fact]
    public async Task CreatePersonAddress_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        Guid nonExistentPersonId = Guid.NewGuid();
        CreatePersonAddressRequest request = new(
            CountryCode.PL,
            _faker.Lorem.Word(),
            _faker.Address.ZipCode("##-###"),
            _faker.Address.State(),
            _faker.Address.City());

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/persons/{nonExistentPersonId}/addresses", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPersonAddress_ShouldReturnAddress_WhenAddressExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        PersonAddressResponse createdAddress = await CreateTestAddressAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/persons/{person.Id.Value}/addresses/{createdAddress.Id.Value}");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        PersonAddressResponse addressResponse = JsonSerializer.Deserialize<PersonAddressResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonAddressResponse");

        addressResponse.ShouldNotBeNull();
        addressResponse.Id.ShouldBe(createdAddress.Id);
        addressResponse.PersonId.ShouldBe(person.Id);
    }

    [Fact]
    public async Task GetPersonAddress_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        Guid nonExistentAddressId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/persons/{person.Id.Value}/addresses/{nonExistentAddressId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPersonAddresses_ShouldReturnAddressesList()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        await CreateTestAddressAsync(person.Id);
        await CreateTestAddressAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/persons/{person.Id.Value}/addresses");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdatePersonAddress_ShouldReturnUpdatedAddress_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        PersonAddressResponse createdAddress = await CreateTestAddressAsync(person.Id);
        UpdatePersonAddressRequest updateRequest = new(
            "Updated Home",
            "00-002",
            "Mazowieckie",
            "Warszawa",
            "ul. Nowa 5");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/persons/{person.Id.Value}/addresses/{createdAddress.Id.Value}", updateRequest);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        PersonAddressResponse addressResponse = JsonSerializer.Deserialize<PersonAddressResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonAddressResponse");

        addressResponse.ShouldNotBeNull();
        addressResponse.Name.ShouldBe("Updated Home");
        addressResponse.PostalCode.ShouldBe("00-002");
        addressResponse.Line.ShouldBe("ul. Nowa 5");
    }

    [Fact]
    public async Task UpdatePersonAddress_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        Guid nonExistentAddressId = Guid.NewGuid();
        UpdatePersonAddressRequest updateRequest = new(
            "Updated Home",
            "00-002",
            "Mazowieckie",
            "Warszawa");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/persons/{person.Id.Value}/addresses/{nonExistentAddressId}", updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldReturnNoContent_WhenAddressExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        PersonAddressResponse createdAddress = await CreateTestAddressAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/persons/{person.Id.Value}/addresses/{createdAddress.Id.Value}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify address is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/persons/{person.Id.Value}/addresses/{createdAddress.Id.Value}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldReturnNotFound_WhenAddressDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        Guid nonExistentAddressId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/persons/{person.Id.Value}/addresses/{nonExistentAddressId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<PersonResponse> CreateTestPersonAsync()
    {
        int number = new Random().Next(100000000, 999999999);
        CreatePersonRequest request = new(
            IdentityId.New(),
            _faker.Internet.UserName(),
            _faker.Internet.Email(),
            $"+48{number}");

        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PersonResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");
    }

    private async Task<PersonAddressResponse> CreateTestAddressAsync(PersonId personId, string? name = null)
    {
        List<(string zipCode, string voivodeship)> polishData =
        [
            ("89-240", "Kujawsko-Pomorskie"),
            ("00-001", "Warmińsko-Mazurskie"),
            ("60-365", "Wielkopolskie")
        ];
        int randomIndex = _faker.Random.Int(0, polishData.Count-1);
        CreatePersonAddressRequest request = new(
            CountryCode.PL,
            name ?? _faker.Lorem.Word(),
            polishData[randomIndex].zipCode,
            polishData[randomIndex].voivodeship,
            _faker.Address.City(),
            _faker.Address.StreetAddress());

        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/persons/{personId.Value}/addresses", request);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PersonAddressResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonAddressResponse");
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

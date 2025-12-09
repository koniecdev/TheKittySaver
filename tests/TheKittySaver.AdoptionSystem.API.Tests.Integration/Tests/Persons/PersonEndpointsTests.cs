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

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

[Collection("Api")]
public sealed class PersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public PersonEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnPerson_WhenValidDataIsProvided()
    {
        // Arrange
        CreatePersonRequest request = new(
            IdentityId.New(),
            "testuser",
            "testuser@example.com",
            $"+48{new Random().Next(100000000, 999999999)}");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        PersonResponse personResponse = JsonSerializer.Deserialize<PersonResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");

        personResponse.ShouldNotBeNull();
        personResponse.Username.ShouldBe(request.Username);
        personResponse.Email.ShouldBe(request.Email);
        personResponse.PhoneNumber.ShouldBe(request.PhoneNumber);
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenInvalidEmailIsProvided()
    {
        // Arrange
        CreatePersonRequest request = new(
            IdentityId.New(),
            _faker.Internet.UserName(),
            "invalid-email",
            $"+48{new Random().Next(100000000, 999999999)}");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenInvalidPhoneNumberIsProvided()
    {
        // Arrange
        CreatePersonRequest request = new(
            IdentityId.New(),
            _faker.Internet.UserName(),
            _faker.Internet.Email(),
            "invalid-phone");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetPerson_ShouldReturnPerson_WhenPersonExists()
    {
        // Arrange
        PersonResponse createdPerson = await CreateTestPersonAsync();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"api/v1/persons/{createdPerson.Id.Value}");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        PersonResponse personResponse = JsonSerializer.Deserialize<PersonResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");

        personResponse.ShouldNotBeNull();
        personResponse.Id.ShouldBe(createdPerson.Id);
        personResponse.Username.ShouldBe(createdPerson.Username);
        personResponse.Email.ShouldBe(createdPerson.Email);
        personResponse.PhoneNumber.ShouldBe(createdPerson.PhoneNumber);
    }

    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"api/v1/persons/{nonExistentId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPersons_ShouldReturnPersonsList()
    {
        // Arrange
        await CreateTestPersonAsync();
        await CreateTestPersonAsync();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("api/v1/persons");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnUpdatedPerson_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse createdPerson = await CreateTestPersonAsync();
        UpdatePersonRequest updateRequest = new(
            "updateduser",
            "updated@example.com",
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/persons/{createdPerson.Id.Value}", updateRequest);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        PersonResponse personResponse = JsonSerializer.Deserialize<PersonResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");

        personResponse.ShouldNotBeNull();
        personResponse.Username.ShouldBe("updateduser");
        personResponse.Email.ShouldBe("updated@example.com");
        personResponse.PhoneNumber.ShouldBe("+48600700800");
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        UpdatePersonRequest updateRequest = new(
            "updateduser",
            "updated@example.com",
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/persons/{nonExistentId}", updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenInvalidEmailIsProvided()
    {
        // Arrange
        PersonResponse createdPerson = await CreateTestPersonAsync();
        UpdatePersonRequest updateRequest = new(
            "updateduser",
            "invalid-email",
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/persons/{createdPerson.Id.Value}", updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeletePerson_ShouldReturnNoContent_WhenPersonExists()
    {
        // Arrange
        PersonResponse createdPerson = await CreateTestPersonAsync();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/persons/{createdPerson.Id.Value}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify person is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync($"api/v1/persons/{createdPerson.Id.Value}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePerson_ShouldReturnNotFound_WhenPersonDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync($"api/v1/persons/{nonExistentId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<PersonResponse> CreateTestPersonAsync()
    {
        CreatePersonRequest request = new(
            IdentityId.New(),
            _faker.Internet.UserName(),
            _faker.Internet.Email(),
            $"+48{new Random().Next(100000000, 999999999)}");

        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PersonResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

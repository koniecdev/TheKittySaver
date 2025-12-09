using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

[Collection("Api")]
public sealed class CatVaccinationEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public CatVaccinationEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreateCatVaccination_ShouldReturnVaccination_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CreateCatVaccinationRequest request = new(
            VaccinationType.Rabies,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            "Annual vaccination");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatVaccinationResponse vaccinationResponse = JsonSerializer.Deserialize<CatVaccinationResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatVaccinationResponse");

        vaccinationResponse.ShouldNotBeNull();
        vaccinationResponse.CatId.ShouldBe(cat.Id);
        vaccinationResponse.Type.ShouldBe(VaccinationType.Rabies);
        vaccinationResponse.VeterinarianNote.ShouldBe("Annual vaccination");
    }

    [Fact]
    public async Task CreateCatVaccination_ShouldReturnVaccination_WhenNoteIsNull()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CreateCatVaccinationRequest request = new(
            VaccinationType.FvrcpPanleukopenia,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatVaccinationResponse vaccinationResponse = JsonSerializer.Deserialize<CatVaccinationResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatVaccinationResponse");

        vaccinationResponse.VeterinarianNote.ShouldBeNull();
    }

    [Fact]
    public async Task CreateCatVaccination_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();
        CreateCatVaccinationRequest request = new(
            VaccinationType.Rabies,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{nonExistentCatId}/vaccinations", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCatVaccination_ShouldReturnMultipleVaccinations_WhenMultipleTypesAdded()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        CreateCatVaccinationRequest rabiesRequest = new(
            VaccinationType.Rabies,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));

        CreateCatVaccinationRequest fvrcpRequest = new(
            VaccinationType.FvrcpCalicivirus,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)));

        // Act
        HttpResponseMessage rabiesResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations", rabiesRequest);
        HttpResponseMessage fvrcpResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations", fvrcpRequest);

        // Assert
        rabiesResponse.EnsureSuccessStatusCode();
        fvrcpResponse.EnsureSuccessStatusCode();

        // Get all vaccinations
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations");
        getResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetCatVaccination_ShouldReturnVaccination_WhenVaccinationExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CatVaccinationResponse createdVaccination = await CreateTestVaccinationAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatVaccinationResponse vaccinationResponse = JsonSerializer.Deserialize<CatVaccinationResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatVaccinationResponse");

        vaccinationResponse.ShouldNotBeNull();
        vaccinationResponse.Id.ShouldBe(createdVaccination.Id);
        vaccinationResponse.CatId.ShouldBe(cat.Id);
    }

    [Fact]
    public async Task GetCatVaccination_ShouldReturnNotFound_WhenVaccinationDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentVaccinationId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations/{nonExistentVaccinationId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCatVaccinations_ShouldReturnVaccinationsList()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        await CreateTestVaccinationAsync(cat.Id, VaccinationType.Rabies);
        await CreateTestVaccinationAsync(cat.Id, VaccinationType.Felv);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task UpdateCatVaccination_ShouldReturnUpdatedVaccination_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CatVaccinationResponse createdVaccination = await CreateTestVaccinationAsync(cat.Id);

        UpdateCatVaccinationRequest updateRequest = new(
            VaccinationType.Felv,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Updated note");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}", updateRequest);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatVaccinationResponse vaccinationResponse = JsonSerializer.Deserialize<CatVaccinationResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatVaccinationResponse");

        vaccinationResponse.ShouldNotBeNull();
        vaccinationResponse.Type.ShouldBe(VaccinationType.Felv);
        vaccinationResponse.VeterinarianNote.ShouldBe("Updated note");
    }

    [Fact]
    public async Task UpdateCatVaccination_ShouldReturnNotFound_WhenVaccinationDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentVaccinationId = Guid.NewGuid();

        UpdateCatVaccinationRequest updateRequest = new(
            VaccinationType.Felv,
            DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations/{nonExistentVaccinationId}", updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCatVaccination_ShouldReturnNoContent_WhenVaccinationExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CatVaccinationResponse createdVaccination = await CreateTestVaccinationAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify vaccination is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCatVaccination_ShouldReturnNotFound_WhenVaccinationDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentVaccinationId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{cat.Id.Value}/vaccinations/{nonExistentVaccinationId}");

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

    private async Task<CatResponse> CreateTestCatAsync(PersonId personId, string? name = null)
    {
        CreateCatRequest request = new(
            personId,
            name ?? _faker.Name.FirstName(),
            _faker.Lorem.Sentence(),
            _faker.Random.Int(1, 15),
            _faker.PickRandom<CatGenderType>(),
            _faker.PickRandom<ColorType>(),
            _faker.Random.Decimal(2.0m, 8.0m),
            HealthStatusType.Healthy,
            false,
            null,
            SpecialNeedsSeverityType.None,
            _faker.PickRandom<TemperamentType>(),
            0,
            null,
            null,
            ListingSourceType.Shelter,
            _faker.Company.CompanyName(),
            true,
            FivStatus.Negative,
            FelvStatus.Negative,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));

        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/cats", request);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CatResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");
    }

    private async Task<CatVaccinationResponse> CreateTestVaccinationAsync(
        CatId catId,
        VaccinationType type = VaccinationType.Rabies)
    {
        CreateCatVaccinationRequest request = new(
            type,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            _faker.Lorem.Sentence());

        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{catId.Value}/vaccinations", request);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CatVaccinationResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatVaccinationResponse");
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;
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
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        CreateCatVaccinationRequest request = new(
            VaccinationType.Rabies,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            "Annual vaccination");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations", UriKind.Relative), request);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

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
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        CreateCatVaccinationRequest request = new(
            VaccinationType.FvrcpPanleukopenia,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations", UriKind.Relative), request);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
            new Uri($"api/v1/cats/{nonExistentCatId}/vaccinations", UriKind.Relative), request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCatVaccination_ShouldReturnMultipleVaccinations_WhenMultipleTypesAdded()
    {
        // Arrange
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);

        CreateCatVaccinationRequest rabiesRequest = new(
            VaccinationType.Rabies,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));

        CreateCatVaccinationRequest fvrcpRequest = new(
            VaccinationType.FvrcpCalicivirus,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-2)));

        // Act
        HttpResponseMessage rabiesResponse = await _httpClient.PostAsJsonAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations", UriKind.Relative), rabiesRequest);
        HttpResponseMessage fvrcpResponse = await _httpClient.PostAsJsonAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations", UriKind.Relative), fvrcpRequest);

        // Assert
        await rabiesResponse.EnsureSuccessWithDetailsAsync();
        await fvrcpResponse.EnsureSuccessWithDetailsAsync();

        // Get cat details with embedded vaccinations
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}", UriKind.Relative));
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatDetailsResponse catResponse = JsonSerializer.Deserialize<CatDetailsResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatDetailsResponse");

        catResponse.Vaccinations.Count.ShouldBe(2);
        catResponse.Vaccinations.ShouldContain(v => v.Type == VaccinationType.Rabies);
        catResponse.Vaccinations.ShouldContain(v => v.Type == VaccinationType.FvrcpCalicivirus);
    }

    [Fact]
    public async Task GetCatVaccination_ShouldReturnVaccination_WhenVaccinationExists()
    {
        // Arrange
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        CatVaccinationResponse createdVaccination = await CreateTestVaccinationAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}", UriKind.Relative));

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentVaccinationId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations/{nonExistentVaccinationId}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCat_ShouldReturnEmbeddedVaccinations_WhenCatHasVaccinations()
    {
        // Arrange
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        await CreateTestVaccinationAsync(cat.Id, VaccinationType.Rabies);
        await CreateTestVaccinationAsync(cat.Id, VaccinationType.Felv);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}", UriKind.Relative));

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatDetailsResponse catResponse = JsonSerializer.Deserialize<CatDetailsResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatDetailsResponse");

        catResponse.Vaccinations.Count.ShouldBe(2);
        catResponse.Vaccinations.ShouldContain(v => v.Type == VaccinationType.Rabies);
        catResponse.Vaccinations.ShouldContain(v => v.Type == VaccinationType.Felv);
    }

    [Fact]
    public async Task UpdateCatVaccination_ShouldReturnUpdatedVaccination_WhenValidDataIsProvided()
    {
        // Arrange
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        CatVaccinationResponse createdVaccination = await CreateTestVaccinationAsync(cat.Id);

        UpdateCatVaccinationRequest updateRequest = new(
            VaccinationType.Felv,
            DateOnly.FromDateTime(DateTime.UtcNow),
            "Updated note");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}", UriKind.Relative), updateRequest);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentVaccinationId = Guid.NewGuid();

        UpdateCatVaccinationRequest updateRequest = new(
            VaccinationType.Felv,
            DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations/{nonExistentVaccinationId}", UriKind.Relative), updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCatVaccination_ShouldReturnNoContent_WhenVaccinationExists()
    {
        // Arrange
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        CatVaccinationResponse createdVaccination = await CreateTestVaccinationAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify vaccination is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations/{createdVaccination.Id.Value}", UriKind.Relative));
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCatVaccination_ShouldReturnNotFound_WhenVaccinationDoesNotExist()
    {
        // Arrange
        PersonDetailsResponse person = await CreateTestPersonAsync();
        CatDetailsResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentVaccinationId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            new Uri($"api/v1/cats/{cat.Id.Value}/vaccinations/{nonExistentVaccinationId}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<PersonDetailsResponse> CreateTestPersonAsync()
        => await PersonApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker);

    private async Task<CatDetailsResponse> CreateTestCatAsync(PersonId personId, string? name = null)
        => await CatApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, personId, name);

    private async Task<CatVaccinationResponse> CreateTestVaccinationAsync(
        CatId catId,
        VaccinationType type = VaccinationType.Rabies)
        => await CatVaccinationApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, catId, type);

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

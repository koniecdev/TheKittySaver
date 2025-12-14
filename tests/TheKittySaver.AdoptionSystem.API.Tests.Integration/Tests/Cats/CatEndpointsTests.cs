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
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

[Collection("Api")]
public sealed class CatEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public CatEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreateCat_ShouldReturnCat_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CreateCatRequest request = CatApiFactory.CreateFixedRequest(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/cats", request);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");

        catResponse.ShouldNotBeNull();
        catResponse.PersonId.ShouldBe(person.Id);
        catResponse.Name.ShouldBe("Mruczek");
        catResponse.Description.ShouldBe("Friendly orange cat");
        catResponse.Age.ShouldBe(3);
        catResponse.Gender.ShouldBe(CatGenderType.Male);
        catResponse.Color.ShouldBe(ColorType.Orange);
        catResponse.WeightValueInKilograms.ShouldBe(4.5m);
        catResponse.HealthStatus.ShouldBe(HealthStatusType.Healthy);
        catResponse.Temperament.ShouldBe(TemperamentType.Friendly);
        catResponse.IsNeutered.ShouldBeTrue();
    }

    [Fact]
    public async Task CreateCat_ShouldReturnCat_WithSpecialNeeds()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CreateCatRequest request = new(
            person.Id,
            "Disabled Cat",
            "Cat with special needs",
            5,
            CatGenderType.Female,
            ColorType.White,
            3.0m,
            HealthStatusType.ChronicIllness,
            true,
            "Needs daily medication",
            SpecialNeedsSeverityType.Moderate,
            TemperamentType.Timid,
            0,
            null,
            null,
            ListingSourceType.Shelter,
            "Test Shelter",
            true,
            FivStatus.Negative,
            FelvStatus.Negative,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/cats", request);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");

        catResponse.SpecialNeedsStatusHasSpecialNeeds.ShouldBeTrue();
        catResponse.SpecialNeedsStatusDescription.ShouldBe("Needs daily medication");
        catResponse.SpecialNeedsStatusSeverityType.ShouldBe(SpecialNeedsSeverityType.Moderate);
    }

    [Fact]
    public async Task CreateCat_ShouldReturnCat_WithAdoptionHistory()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        DateTimeOffset lastReturnDate = DateTimeOffset.UtcNow.AddMonths(-2);
        CreateCatRequest request = new(
            person.Id,
            "Returned Cat",
            "Cat that was returned",
            4,
            CatGenderType.Male,
            ColorType.Gray,
            5.0m,
            HealthStatusType.Healthy,
            false,
            null,
            SpecialNeedsSeverityType.None,
            TemperamentType.Independent,
            2,
            lastReturnDate,
            "Owner allergies",
            ListingSourceType.Foundation,
            "Cat Foundation",
            true,
            FivStatus.Negative,
            FelvStatus.Negative,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/cats", request);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");

        catResponse.AdoptionHistoryReturnCount.ShouldBe(2);
        catResponse.AdoptionHistoryLastReturnReason.ShouldBe("Owner allergies");
    }

    [Fact]
    public async Task CreateCat_ShouldReturnBadRequest_WhenPersonDoesNotExist()
    {
        // Arrange
        PersonId nonExistentPersonId = new(Guid.NewGuid());
        CreateCatRequest request = CatApiFactory.CreateFixedRequest(nonExistentPersonId);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/cats", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCat_ShouldReturnCat_WhenCatExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse createdCat = await CreateTestCatAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"api/v1/cats/{createdCat.Id.Value}");

        int x = 1;
        
        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");

        catResponse.ShouldNotBeNull();
        catResponse.Id.ShouldBe(createdCat.Id);
        catResponse.Name.ShouldBe(createdCat.Name);
    }

    [Fact]
    public async Task GetCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"api/v1/cats/{nonExistentId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCats_ShouldReturnCatsList()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        await CreateTestCatAsync(person.Id);
        await CreateTestCatAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("api/v1/cats");

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnUpdatedCat_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse createdCat = await CreateTestCatAsync(person.Id);
        UpdateCatRequest updateRequest = new(
            "Updated Name",
            "Updated description",
            4,
            CatGenderType.Male,
            ColorType.Black,
            5.5m,
            HealthStatusType.MinorIssues,
            false,
            null,
            SpecialNeedsSeverityType.None,
            TemperamentType.Independent,
            0,
            null,
            null,
            ListingSourceType.Foundation,
            "Updated Shelter",
            true,
            FivStatus.Negative,
            FelvStatus.Negative,
            DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{createdCat.Id.Value}", updateRequest);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        CatResponse catResponse = JsonSerializer.Deserialize<CatResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");

        catResponse.ShouldNotBeNull();
        catResponse.Name.ShouldBe("Updated Name");
        catResponse.Description.ShouldBe("Updated description");
        catResponse.Age.ShouldBe(4);
        catResponse.Color.ShouldBe(ColorType.Black);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        UpdateCatRequest updateRequest = new(
            "Updated Name",
            "Updated description",
            4,
            CatGenderType.Male,
            ColorType.Black,
            5.5m,
            HealthStatusType.Healthy,
            false,
            null,
            SpecialNeedsSeverityType.None,
            TemperamentType.Friendly,
            0,
            null,
            null,
            ListingSourceType.Shelter,
            "Test Shelter",
            true,
            FivStatus.Negative,
            FelvStatus.Negative,
            DateOnly.FromDateTime(DateTime.UtcNow));

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{nonExistentId}", updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCat_ShouldReturnNoContent_WhenCatExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse createdCat = await CreateTestCatAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync($"api/v1/cats/{createdCat.Id.Value}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify cat is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync($"api/v1/cats/{createdCat.Id.Value}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync($"api/v1/cats/{nonExistentId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<PersonResponse> CreateTestPersonAsync()
        => await PersonApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker);

    private async Task<CatResponse> CreateTestCatAsync(PersonId personId, string? name = null)
        => await CatApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, personId, name);

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

[Collection("Api")]
public sealed class CatAssignmentEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public CatAssignmentEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task AssignCat_ShouldReturnSuccess_WhenCatAndAnnouncementExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        AssignCatRequest request = new(announcement.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/assign", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        // Verify cat is assigned
        CatResponse updatedCat = await GetCatAsync(cat.Id);
        updatedCat.AdoptionAnnouncementId.ShouldBe(announcement.Id);
    }

    [Fact]
    public async Task AssignCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        Guid nonExistentCatId = Guid.NewGuid();
        AssignCatRequest request = new(announcement.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{nonExistentCatId}/assign", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task AssignCat_ShouldReturnNotFound_WhenAnnouncementDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        AdoptionAnnouncementId nonExistentAnnouncementId = new(Guid.NewGuid());
        AssignCatRequest request = new(nonExistentAnnouncementId);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/assign", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReassignCat_ShouldReturnSuccess_WhenCatIsAlreadyAssigned()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement1 = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        // Assign cat first
        AssignCatRequest assignRequest = new(announcement1.Id);
        HttpResponseMessage assignResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/assign", assignRequest);
        assignResponse.EnsureSuccessStatusCode();

        // Create second cat and announcement for reassignment
        CatResponse cat2 = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement2 = await CreateTestAdoptionAnnouncementAsync(cat2.Id);

        ReassignCatRequest reassignRequest = new(announcement2.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/reassign", reassignRequest);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        // Verify cat is reassigned
        CatResponse updatedCat = await GetCatAsync(cat.Id);
        updatedCat.AdoptionAnnouncementId.ShouldBe(announcement2.Id);
    }

    [Fact]
    public async Task ReassignCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        Guid nonExistentCatId = Guid.NewGuid();
        ReassignCatRequest request = new(announcement.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{nonExistentCatId}/reassign", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnassignCat_ShouldReturnSuccess_WhenCatIsAssigned()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        // Assign cat first
        AssignCatRequest assignRequest = new(announcement.Id);
        HttpResponseMessage assignResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/assign", assignRequest);
        assignResponse.EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/unassign", null);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        // Verify cat is unassigned
        CatResponse updatedCat = await GetCatAsync(cat.Id);
        updatedCat.AdoptionAnnouncementId.ShouldBeNull();
    }

    [Fact]
    public async Task UnassignCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{nonExistentCatId}/unassign", null);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClaimCat_ShouldReturnSuccess_WhenCatIsAssigned()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        // Assign cat first
        AssignCatRequest assignRequest = new(announcement.Id);
        HttpResponseMessage assignResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/assign", assignRequest);
        assignResponse.EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/claim", null);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ClaimCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{nonExistentCatId}/claim", null);

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

    private async Task<AdoptionAnnouncementResponse> CreateTestAdoptionAnnouncementAsync(CatId catId)
    {
        CreateAdoptionAnnouncementRequest request = new(
            catId,
            _faker.Lorem.Paragraph(),
            CountryCode.PL,
            _faker.Address.ZipCode("##-###"),
            _faker.Address.State(),
            _faker.Address.City(),
            _faker.Address.StreetAddress(),
            _faker.Internet.Email(),
            "+48600700800");

        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            "api/v1/adoption-announcements", request);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<AdoptionAnnouncementResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize AdoptionAnnouncementResponse");
    }

    private async Task<CatResponse> GetCatAsync(CatId catId)
    {
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync($"api/v1/cats/{catId.Value}");
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CatResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatResponse");
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

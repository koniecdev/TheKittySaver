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
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

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
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
        await assignResponse.EnsureSuccessWithDetailsAsync();

        // Create second cat and announcement for reassignment
        CatResponse cat2 = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementResponse announcement2 = await CreateTestAdoptionAnnouncementAsync(cat2.Id);

        ReassignCatRequest reassignRequest = new(announcement2.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/reassign", reassignRequest);

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
        await assignResponse.EnsureSuccessWithDetailsAsync();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/unassign", null);

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
        await assignResponse.EnsureSuccessWithDetailsAsync();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/claim", null);

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();
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
        => await PersonApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker);

    private async Task<CatResponse> CreateTestCatAsync(PersonId personId, string? name = null)
        => await CatApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, personId, name);

    private async Task<AdoptionAnnouncementResponse> CreateTestAdoptionAnnouncementAsync(CatId catId)
        => await AdoptionAnnouncementApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, catId);

    private async Task<CatResponse> GetCatAsync(CatId catId)
        => await CatApiFactory.GetAsync(_httpClient, _jsonSerializerOptions, catId);

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

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
        CatResponse catForAnnouncement = await CreateTestCatWithThumbnailAsync(person.Id, "AnnouncementCat");
        CatResponse catToAssign = await CreateTestCatWithThumbnailAsync(person.Id, "CatToAssign");
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(catForAnnouncement.Id);

        AssignCatRequest request = new(announcement.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{catToAssign.Id.Value}/assignment", request);

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        // Verify cat is assigned
        CatResponse updatedCat = await GetCatAsync(catToAssign.Id);
        updatedCat.AdoptionAnnouncementId.ShouldBe(announcement.Id);
    }

    [Fact]
    public async Task AssignCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse catForAnnouncement = await CreateTestCatWithThumbnailAsync(person.Id);
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(catForAnnouncement.Id);

        Guid nonExistentCatId = Guid.NewGuid();
        AssignCatRequest request = new(announcement.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{nonExistentCatId}/assignment", request);

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
            $"api/v1/cats/{cat.Id.Value}/assignment", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReassignCat_ShouldReturnSuccess_WhenCatIsAlreadyAssigned()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse catNamedKrowka = await CreateTestCatWithThumbnailAsync(person.Id, "Krówka");
        CatResponse catNamedFiga = await CreateTestCatWithThumbnailAsync(person.Id, "Figa");
        AdoptionAnnouncementResponse aaWithKrowkaInitially = await CreateTestAdoptionAnnouncementAsync(catNamedKrowka.Id);

        AssignCatRequest assignRequestForKrowkaAa = new(aaWithKrowkaInitially.Id);
        HttpResponseMessage assignFigaToKrowkaAaResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{catNamedFiga.Id.Value}/assignment", assignRequestForKrowkaAa);
        await assignFigaToKrowkaAaResponse.EnsureSuccessWithDetailsAsync();

        // Create second announcement for reassignment
        CatResponse catNamedKlementynka = await CreateTestCatWithThumbnailAsync(person.Id, "Klementynka");
        AdoptionAnnouncementResponse aaWithKlementynkaInitially = await CreateTestAdoptionAnnouncementAsync(catNamedKlementynka.Id);

        ReassignCatRequest reassignFigaToAaWithKlementynkaRequest = new(aaWithKlementynkaInitially.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{catNamedFiga.Id.Value}/assignment", reassignFigaToAaWithKlementynkaRequest);

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        // Verify cat is reassigned
        CatResponse updatedCat = await GetCatAsync(catNamedFiga.Id);
        updatedCat.AdoptionAnnouncementId.ShouldBe(aaWithKlementynkaInitially.Id);
    }

    [Fact]
    public async Task ReassignCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse catForAnnouncement = await CreateTestCatWithThumbnailAsync(person.Id);
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(catForAnnouncement.Id);

        Guid nonExistentCatId = Guid.NewGuid();
        ReassignCatRequest request = new(announcement.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{nonExistentCatId}/assignment", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UnassignCat_ShouldReturnSuccess_WhenCatIsAssigned()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse catForAnnouncement = await CreateTestCatWithThumbnailAsync(person.Id, "AnnCat");
        CatResponse catToAssign = await CreateTestCatWithThumbnailAsync(person.Id, "CatToAssign");
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(catForAnnouncement.Id);

        // Assign cat first
        AssignCatRequest assignRequest = new(announcement.Id);
        HttpResponseMessage assignResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{catToAssign.Id.Value}/assignment", assignRequest);
        await assignResponse.EnsureSuccessWithDetailsAsync();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{catToAssign.Id.Value}/assignment");

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        // Verify cat is unassigned
        CatResponse updatedCat = await GetCatAsync(catToAssign.Id);
        updatedCat.AdoptionAnnouncementId.ShouldBeNull();
    }

    [Fact]
    public async Task UnassignCat_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{nonExistentCatId}/assignment");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClaimCat_ShouldReturnSuccess_WhenCatIsAssigned()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse catForAnnouncement = await CreateTestCatWithThumbnailAsync(person.Id, "AnnCat");
        CatResponse catToAssign = await CreateTestCatWithThumbnailAsync(person.Id, "CatToAssign");
        AdoptionAnnouncementResponse announcement = await CreateTestAdoptionAnnouncementAsync(catForAnnouncement.Id);

        // Assign cat first
        AssignCatRequest assignRequest = new(announcement.Id);
        HttpResponseMessage assignResponse = await _httpClient.PostAsJsonAsync(
            $"api/v1/cats/{catToAssign.Id.Value}/assignment", assignRequest);
        await assignResponse.EnsureSuccessWithDetailsAsync();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{catToAssign.Id.Value}/claim", null);

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

    private async Task<CatResponse> CreateTestCatWithThumbnailAsync(PersonId personId, string? name = null)
        => await CatApiFactory.CreateHealthyCatWithThumbnail(_httpClient, _jsonSerializerOptions, personId, name);

    private async Task<AdoptionAnnouncementResponse> CreateTestAdoptionAnnouncementAsync(CatId catId)
        => await AdoptionAnnouncementApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, catId);

    private async Task<CatResponse> GetCatAsync(CatId catId)
        => await CatApiFactory.GetAsync(_httpClient, _jsonSerializerOptions, catId);

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

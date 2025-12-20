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
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

[Collection("Api")]
public sealed class AdoptionAnnouncementEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public AdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnAnnouncement_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        CreateAdoptionAnnouncementRequest request = new(
            cat.Id,
            "Looking for a loving home for this adorable cat!",
            CountryCode.PL,
            "00-001",
            "Mazowieckie",
            "Warszawa",
            "ul. Testowa 1",
            "contact@shelter.pl",
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            "api/v1/adoption-announcements", request);
        
        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        AdoptionAnnouncementListItemResponse announcementListItemResponse = JsonSerializer.Deserialize<AdoptionAnnouncementListItemResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize AdoptionAnnouncementResponse");

        announcementListItemResponse.ShouldNotBeNull();
        announcementListItemResponse.PersonId.ShouldBe(person.Id);
        announcementListItemResponse.Description.ShouldBe("Looking for a loving home for this adorable cat!");
        announcementListItemResponse.AddressCountryCode.ShouldBe(CountryCode.PL);
        announcementListItemResponse.AddressPostalCode.ShouldBe("00-001");
        announcementListItemResponse.AddressRegion.ShouldBe("Mazowieckie");
        announcementListItemResponse.AddressCity.ShouldBe("Warszawa");
        announcementListItemResponse.AddressLine.ShouldBe("ul. Testowa 1");
        announcementListItemResponse.Email.ShouldBe("contact@shelter.pl");
        announcementListItemResponse.PhoneNumber.ShouldBe("+48600700800");
        announcementListItemResponse.Status.ShouldBe(AnnouncementStatusType.Active);
    }

    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnAnnouncement_WhenDescriptionIsNull()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        CreateAdoptionAnnouncementRequest request = new(
            cat.Id,
            null,
            CountryCode.PL,
            "60-365",
            "Wielkopolskie",
            _faker.Address.City(),
            null,
            _faker.Internet.Email(),
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            "api/v1/adoption-announcements", request);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        AdoptionAnnouncementListItemResponse announcementListItemResponse = JsonSerializer.Deserialize<AdoptionAnnouncementListItemResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize AdoptionAnnouncementResponse");

        announcementListItemResponse.Description.ShouldBeNull();
        announcementListItemResponse.AddressLine.ShouldBeNull();
    }

    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        CatId nonExistentCatId = new(Guid.NewGuid());

        CreateAdoptionAnnouncementRequest request = new(
            nonExistentCatId,
            _faker.Lorem.Sentence(10),
            CountryCode.PL,
            "30-001",
            "Małopolskie",
            _faker.Address.City(),
            null,
            _faker.Internet.Email(),
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            "api/v1/adoption-announcements", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnBadRequest_WhenInvalidEmailIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        CreateAdoptionAnnouncementRequest request = new(
            cat.Id,
            _faker.Lorem.Sentence(10),
            CountryCode.PL,
            "80-001",
            "Pomorskie",
            _faker.Address.City(),
            null,
            "invalid-email",
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            "api/v1/adoption-announcements", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnBadRequest_WhenInvalidPhoneNumberIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        CreateAdoptionAnnouncementRequest request = new(
            cat.Id,
            _faker.Lorem.Sentence(10),
            CountryCode.PL,
            "10-001",
            "Warmińsko-Mazurskie",
            _faker.Address.City(),
            null,
            _faker.Internet.Email(),
            "invalid-phone");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync(
            "api/v1/adoption-announcements", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldReturnAnnouncement_WhenAnnouncementExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementListItemResponse createdAnnouncementListItem = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/adoption-announcements/{createdAnnouncementListItem.Id.Value}");

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        AdoptionAnnouncementListItemResponse announcementListItemResponse = JsonSerializer.Deserialize<AdoptionAnnouncementListItemResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize AdoptionAnnouncementResponse");

        announcementListItemResponse.ShouldNotBeNull();
        announcementListItemResponse.Id.ShouldBe(createdAnnouncementListItem.Id);
    }

    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldReturnNotFound_WhenAnnouncementDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/adoption-announcements/{nonExistentId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAdoptionAnnouncements_ShouldReturnAnnouncementsList()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat1 = await CreateTestCatAsync(person.Id);
        CatResponse cat2 = await CreateTestCatAsync(person.Id);
        await CreateTestAdoptionAnnouncementAsync(cat1.Id);
        await CreateTestAdoptionAnnouncementAsync(cat2.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync("api/v1/adoption-announcements");

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();
    }

    [Fact]
    public async Task UpdateAdoptionAnnouncement_ShouldReturnUpdatedAnnouncement_WhenValidDataIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementListItemResponse createdAnnouncementListItem = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        UpdateAdoptionAnnouncementRequest updateRequest = new(
            "Updated description",
            CountryCode.DE,
            "10115",
            "Berlin",
            "Berlin",
            "Musterstraße 1",
            "updated@shelter.pl",
            "+49301234567");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/adoption-announcements/{createdAnnouncementListItem.Id.Value}", updateRequest);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        AdoptionAnnouncementListItemResponse announcementListItemResponse = JsonSerializer.Deserialize<AdoptionAnnouncementListItemResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize AdoptionAnnouncementResponse");

        announcementListItemResponse.ShouldNotBeNull();
        announcementListItemResponse.Description.ShouldBe("Updated description");
        announcementListItemResponse.AddressCountryCode.ShouldBe(CountryCode.DE);
        announcementListItemResponse.AddressPostalCode.ShouldBe("10115");
        announcementListItemResponse.AddressCity.ShouldBe("Berlin");
        announcementListItemResponse.Email.ShouldBe("updated@shelter.pl");
        announcementListItemResponse.PhoneNumber.ShouldBe("+49301234567");
    }

    [Fact]
    public async Task UpdateAdoptionAnnouncement_ShouldReturnNotFound_WhenAnnouncementDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();
        UpdateAdoptionAnnouncementRequest updateRequest = new(
            _faker.Lorem.Sentence(10),
            CountryCode.PL,
            "89-240",
            "Kujawsko-Pomorskie",
            _faker.Address.City(),
            null,
            _faker.Internet.Email(),
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/adoption-announcements/{nonExistentId}", updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAdoptionAnnouncement_ShouldReturnBadRequest_WhenInvalidEmailIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementListItemResponse createdAnnouncementListItem = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        UpdateAdoptionAnnouncementRequest updateRequest = new(
            _faker.Lorem.Sentence(10),
            CountryCode.PL,
            "00-001",
            "Mazowieckie",
            _faker.Address.City(),
            null,
            "invalid-email",
            "+48600700800");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/adoption-announcements/{createdAnnouncementListItem.Id.Value}", updateRequest);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteAdoptionAnnouncement_ShouldReturnNoContent_WhenAnnouncementExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementListItemResponse createdAnnouncementListItem = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/adoption-announcements/{createdAnnouncementListItem.Id.Value}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify announcement is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/adoption-announcements/{createdAnnouncementListItem.Id.Value}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAdoptionAnnouncement_ShouldReturnNotFound_WhenAnnouncementDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/adoption-announcements/{nonExistentId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ClaimAdoptionAnnouncement_ShouldReturnSuccess_WhenAnnouncementIsActive()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        AdoptionAnnouncementListItemResponse announcementListItem = await CreateTestAdoptionAnnouncementAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/adoption-announcements/{announcementListItem.Id.Value}/claim", null);

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        // Verify announcement is claimed
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/adoption-announcements/{announcementListItem.Id.Value}");
        await getResponse.EnsureSuccessWithDetailsAsync();

        string stringResponse = await getResponse.Content.ReadAsStringAsync();
        AdoptionAnnouncementListItemResponse claimedAnnouncementListItem = JsonSerializer.Deserialize<AdoptionAnnouncementListItemResponse>(stringResponse, _jsonSerializerOptions)!;
        claimedAnnouncementListItem.Status.ShouldBe(AnnouncementStatusType.Claimed);
    }

    [Fact]
    public async Task ClaimAdoptionAnnouncement_ShouldReturnNotFound_WhenAnnouncementDoesNotExist()
    {
        // Arrange
        Guid nonExistentId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/adoption-announcements/{nonExistentId}/claim", null);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<PersonResponse> CreateTestPersonAsync()
        => await PersonApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker);

    private async Task<CatResponse> CreateTestCatAsync(PersonId personId, string? name = null)
        => await CatApiFactory.CreateHealthyCatWithThumbnail(_httpClient, _jsonSerializerOptions, personId, name);

    private async Task<AdoptionAnnouncementListItemResponse> CreateTestAdoptionAnnouncementAsync(CatId catId)
        => await AdoptionAnnouncementApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, catId);

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

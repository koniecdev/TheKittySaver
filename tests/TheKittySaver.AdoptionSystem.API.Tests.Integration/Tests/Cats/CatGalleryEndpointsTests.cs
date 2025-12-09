using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

[Collection("Api")]
public sealed class CatGalleryEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public CatGalleryEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreateCatGalleryItem_ShouldReturnGalleryItem_WhenValidFileIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        using MultipartFormDataContent content = CreateTestImageContent();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery", content);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatGalleryItemResponse galleryItemResponse = JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatGalleryItemResponse");

        galleryItemResponse.ShouldNotBeNull();
        galleryItemResponse.CatId.ShouldBe(cat.Id);
        galleryItemResponse.DisplayOrder.ShouldBe(0);
    }

    [Fact]
    public async Task CreateCatGalleryItem_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();
        using MultipartFormDataContent content = CreateTestImageContent();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{nonExistentCatId}/gallery", content);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateCatGalleryItem_ShouldCreateMultipleItems_WhenMultipleFilesAdded()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        // Act
        using MultipartFormDataContent content1 = CreateTestImageContent();
        HttpResponseMessage response1 = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery", content1);

        using MultipartFormDataContent content2 = CreateTestImageContent();
        HttpResponseMessage response2 = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery", content2);

        // Assert
        response1.EnsureSuccessStatusCode();
        response2.EnsureSuccessStatusCode();

        string stringResponse1 = await response1.Content.ReadAsStringAsync();
        CatGalleryItemResponse item1 = JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse1, _jsonSerializerOptions)!;

        string stringResponse2 = await response2.Content.ReadAsStringAsync();
        CatGalleryItemResponse item2 = JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse2, _jsonSerializerOptions)!;

        item1.DisplayOrder.ShouldBe(0);
        item2.DisplayOrder.ShouldBe(1);
    }

    [Fact]
    public async Task GetCatGalleryItem_ShouldReturnGalleryItem_WhenItemExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CatGalleryItemResponse createdItem = await CreateTestGalleryItemAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery/{createdItem.Id.Value}");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatGalleryItemResponse galleryItemResponse = JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatGalleryItemResponse");

        galleryItemResponse.ShouldNotBeNull();
        galleryItemResponse.Id.ShouldBe(createdItem.Id);
        galleryItemResponse.CatId.ShouldBe(cat.Id);
    }

    [Fact]
    public async Task GetCatGalleryItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentItemId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery/{nonExistentItemId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCatGalleryItems_ShouldReturnGalleryItemsList()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        await CreateTestGalleryItemAsync(cat.Id);
        await CreateTestGalleryItemAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetCatGalleryItemFile_ShouldReturnFile_WhenItemExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CatGalleryItemResponse createdItem = await CreateTestGalleryItemAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery/{createdItem.Id.Value}/file");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        httpResponseMessage.Content.Headers.ContentType?.MediaType.ShouldBe("image/png");
    }

    [Fact]
    public async Task DeleteCatGalleryItem_ShouldReturnNoContent_WhenItemExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CatGalleryItemResponse createdItem = await CreateTestGalleryItemAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery/{createdItem.Id.Value}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify item is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery/{createdItem.Id.Value}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCatGalleryItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        Guid nonExistentItemId = Guid.NewGuid();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery/{nonExistentItemId}");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ReorderCatGallery_ShouldReorderItems_WhenValidOrderIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        CatGalleryItemResponse item1 = await CreateTestGalleryItemAsync(cat.Id);
        CatGalleryItemResponse item2 = await CreateTestGalleryItemAsync(cat.Id);

        ReorderCatGalleryRequest request = new([
            new GalleryItemOrderEntry(item1.Id.Value, 1),
            new GalleryItemOrderEntry(item2.Id.Value, 0)
        ]);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery/reorder", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        List<CatGalleryItemResponse>? reorderedItems = JsonSerializer.Deserialize<List<CatGalleryItemResponse>>(stringResponse, _jsonSerializerOptions);

        reorderedItems.ShouldNotBeNull();
        reorderedItems.ShouldContain(i => i.Id == item1.Id && i.DisplayOrder == 1);
        reorderedItems.ShouldContain(i => i.Id == item2.Id && i.DisplayOrder == 0);
    }

    [Fact]
    public async Task ReorderCatGallery_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();
        ReorderCatGalleryRequest request = new([
            new GalleryItemOrderEntry(Guid.NewGuid(), 0)
        ]);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsJsonAsync(
            $"api/v1/cats/{nonExistentCatId}/gallery/reorder", request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpsertCatThumbnail_ShouldReturnThumbnail_WhenValidFileIsProvided()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        using MultipartFormDataContent content = CreateTestImageContent();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail", content);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatThumbnailResponse thumbnailResponse = JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatThumbnailResponse");

        thumbnailResponse.ShouldNotBeNull();
        thumbnailResponse.CatId.ShouldBe(cat.Id);
    }

    [Fact]
    public async Task UpsertCatThumbnail_ShouldReplaceThumbnail_WhenThumbnailAlreadyExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        // Create first thumbnail
        using MultipartFormDataContent content1 = CreateTestImageContent();
        HttpResponseMessage response1 = await _httpClient.PutAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail", content1);
        response1.EnsureSuccessStatusCode();

        string stringResponse1 = await response1.Content.ReadAsStringAsync();
        CatThumbnailResponse thumbnail1 = JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse1, _jsonSerializerOptions)!;

        // Act - Replace thumbnail
        using MultipartFormDataContent content2 = CreateTestImageContent();
        HttpResponseMessage response2 = await _httpClient.PutAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail", content2);

        // Assert
        response2.EnsureSuccessStatusCode();

        string stringResponse2 = await response2.Content.ReadAsStringAsync();
        CatThumbnailResponse thumbnail2 = JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse2, _jsonSerializerOptions)!;

        thumbnail2.CatId.ShouldBe(cat.Id);
    }

    [Fact]
    public async Task UpsertCatThumbnail_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();
        using MultipartFormDataContent content = CreateTestImageContent();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(
            $"api/v1/cats/{nonExistentCatId}/thumbnail", content);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCatThumbnail_ShouldReturnThumbnail_WhenThumbnailExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        await CreateTestThumbnailAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        CatThumbnailResponse thumbnailResponse = JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatThumbnailResponse");

        thumbnailResponse.ShouldNotBeNull();
        thumbnailResponse.CatId.ShouldBe(cat.Id);
    }

    [Fact]
    public async Task GetCatThumbnail_ShouldReturnNotFound_WhenThumbnailDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCatThumbnailFile_ShouldReturnFile_WhenThumbnailExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        await CreateTestThumbnailAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail/file");

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        httpResponseMessage.Content.Headers.ContentType?.MediaType.ShouldBe("image/png");
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

    private async Task<CatGalleryItemResponse> CreateTestGalleryItemAsync(CatId catId)
    {
        using MultipartFormDataContent content = CreateTestImageContent();

        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{catId.Value}/gallery", content);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatGalleryItemResponse");
    }

    private async Task<CatThumbnailResponse> CreateTestThumbnailAsync(CatId catId)
    {
        using MultipartFormDataContent content = CreateTestImageContent();

        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(
            $"api/v1/cats/{catId.Value}/thumbnail", content);
        httpResponseMessage.EnsureSuccessStatusCode();

        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatThumbnailResponse");
    }

    private static MultipartFormDataContent CreateTestImageContent()
    {
        byte[] pngBytes =
        [
            0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A,
            0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52,
            0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x01,
            0x08, 0x06, 0x00, 0x00, 0x00, 0x1F, 0x15, 0xC4,
            0x89, 0x00, 0x00, 0x00, 0x0A, 0x49, 0x44, 0x41,
            0x54, 0x78, 0x9C, 0x63, 0x00, 0x01, 0x00, 0x00,
            0x05, 0x00, 0x01, 0x0D, 0x0A, 0x2D, 0xB4, 0x00,
            0x00, 0x00, 0x00, 0x49, 0x45, 0x4E, 0x44, 0xAE,
            0x42, 0x60, 0x82
        ];

        using ByteArrayContent fileContent = new(pngBytes);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");

        MultipartFormDataContent content = new()
        {
            { fileContent, "file", "test.png" }
        };

        return content;
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

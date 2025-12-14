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
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
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

        using MultipartFormDataContent content = ImageContentFactory.CreateTestPngContent();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery", content);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

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
        using MultipartFormDataContent content = ImageContentFactory.CreateTestPngContent();

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
        using MultipartFormDataContent content1 = ImageContentFactory.CreateTestPngContent();
        HttpResponseMessage response1 = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery", content1);

        using MultipartFormDataContent content2 = ImageContentFactory.CreateTestPngContent();
        HttpResponseMessage response2 = await _httpClient.PostAsync(
            $"api/v1/cats/{cat.Id.Value}/gallery", content2);

        // Assert
        string stringResponse1 = await response1.EnsureSuccessWithDetailsAsync();
        string stringResponse2 = await response2.EnsureSuccessWithDetailsAsync();

        CatGalleryItemResponse item1 = JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse1, _jsonSerializerOptions)!;
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
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();
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
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();
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
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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

        using MultipartFormDataContent content = ImageContentFactory.CreateTestPngContent();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail", content);

        // Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

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
        using MultipartFormDataContent content1 = ImageContentFactory.CreateTestPngContent();
        HttpResponseMessage response1 = await _httpClient.PutAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail", content1);
        string stringResponse1 = await response1.EnsureSuccessWithDetailsAsync();

        CatThumbnailResponse thumbnail1 = JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse1, _jsonSerializerOptions)!;

        // Act - Replace thumbnail
        using MultipartFormDataContent content2 = ImageContentFactory.CreateTestPngContent();
        HttpResponseMessage response2 = await _httpClient.PutAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail", content2);

        // Assert
        string stringResponse2 = await response2.EnsureSuccessWithDetailsAsync();

        CatThumbnailResponse thumbnail2 = JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse2, _jsonSerializerOptions)!;

        thumbnail2.CatId.ShouldBe(cat.Id);
    }

    [Fact]
    public async Task UpsertCatThumbnail_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        // Arrange
        Guid nonExistentCatId = Guid.NewGuid();
        using MultipartFormDataContent content = ImageContentFactory.CreateTestPngContent();

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PutAsync(
            $"api/v1/cats/{nonExistentCatId}/thumbnail", content);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCatThumbnail_ShouldReturnFile_WhenThumbnailExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        await CreateTestThumbnailAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail");

        // Assert
        await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.Content.Headers.ContentType?.MediaType.ShouldBe("image/png");
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
    public async Task DeleteCatThumbnail_ShouldReturnNoContent_WhenThumbnailExists()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);
        await CreateTestThumbnailAsync(cat.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Verify thumbnail is deleted
        HttpResponseMessage getResponse = await _httpClient.GetAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCatThumbnail_ShouldReturnNotFound_WhenThumbnailDoesNotExist()
    {
        // Arrange
        PersonResponse person = await CreateTestPersonAsync();
        CatResponse cat = await CreateTestCatAsync(person.Id);

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.DeleteAsync(
            $"api/v1/cats/{cat.Id.Value}/thumbnail");

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<PersonResponse> CreateTestPersonAsync()
        => await PersonApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker);

    private async Task<CatResponse> CreateTestCatAsync(PersonId personId, string? name = null)
        => await CatApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, _faker, personId, name);

    private async Task<CatGalleryItemResponse> CreateTestGalleryItemAsync(CatId catId)
        => await CatGalleryApiFactory.CreateRandomAsync(_httpClient, _jsonSerializerOptions, catId);

    private async Task<CatThumbnailResponse> CreateTestThumbnailAsync(CatId catId)
        => await CatGalleryApiFactory.CreateThumbnailAsync(_httpClient, _jsonSerializerOptions, catId);

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

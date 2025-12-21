using System.Text.Json;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class CatGalleryApiFactory
{
    public static async Task<CatGalleryItemResponse> CreateRandomAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        CatId catId)
    {
        using MultipartFormDataContent content = ImageContentFactory.CreateTestPngContent();

        HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(
            new Uri($"api/v1/cats/{catId.Value}/gallery", UriKind.Relative), content);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        CatGalleryItemResponse galleryItemResponse = JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatGalleryItemResponse");
        return galleryItemResponse;
    }

    public static async Task<CatThumbnailResponse> CreateThumbnailAsync(
        HttpClient httpClient,
        JsonSerializerOptions jsonSerializerOptions,
        CatId catId)
    {
        using MultipartFormDataContent content = ImageContentFactory.CreateTestPngContent();

        HttpResponseMessage httpResponseMessage = await httpClient.PutAsync(
            new Uri($"api/v1/cats/{catId.Value}/thumbnail", UriKind.Relative), content);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        CatThumbnailResponse thumbnailResponse = JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse, jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize CatThumbnailResponse");
        return thumbnailResponse;
    }
}

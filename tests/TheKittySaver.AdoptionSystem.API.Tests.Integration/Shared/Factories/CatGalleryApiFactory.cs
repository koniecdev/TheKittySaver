using System.Net.Http.Headers;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class CatGalleryApiFactory
{
    public static async Task<CatGalleryItemResponse> CreateRandomAsync(TestApiClient apiClient, CatId catId)
    {
        byte[] imageBytes = ImageContentFactory.CreateMinimalPng();

        using MultipartFormDataContent content = [];
        using ByteArrayContent fileContent = new(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "test-image.png");

        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PostAsync(new Uri($"api/v1/cats/{catId}/gallery", UriKind.Relative), content);

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<CatGalleryItemResponse>(stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize CatGalleryItemResponse");
    }

    public static async Task<CatThumbnailResponse> UpsertThumbnailAsync(TestApiClient apiClient, CatId catId)
    {
        byte[] imageBytes = ImageContentFactory.CreateMinimalPng();

        using MultipartFormDataContent content = [];
        using ByteArrayContent fileContent = new(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "test-thumbnail.png");

        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.PutAsync(new Uri($"api/v1/cats/{catId}/thumbnail", UriKind.Relative), content);

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<CatThumbnailResponse>(stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize CatThumbnailResponse");
    }
}

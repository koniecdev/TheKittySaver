using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Cats;

internal static class CatApiQueryService
{
    public static async Task<CatDetailsResponse> GetByIdAsync(TestApiClient apiClient, CatId catId)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.GetAsync(new Uri($"api/v1/cats/{catId.Value}", UriKind.Relative));

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<CatDetailsResponse>(stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize CatDetailsResponse");
    }

    public static async Task<PaginationResponse<CatListItemResponse>> GetAllAsync(TestApiClient apiClient)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.GetAsync(new Uri("api/v1/cats", UriKind.Relative));

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<PaginationResponse<CatListItemResponse>>(
                   stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize PaginationResponse");
    }
}

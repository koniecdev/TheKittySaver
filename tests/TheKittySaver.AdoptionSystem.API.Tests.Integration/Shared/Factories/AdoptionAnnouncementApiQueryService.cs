using System.Text.Json;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;

internal static class AdoptionAnnouncementApiQueryService
{
    public static async Task<AdoptionAnnouncementDetailsResponse> GetByIdAsync(
        TestApiClient apiClient,
        AdoptionAnnouncementId announcementId)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.GetAsync(new Uri($"api/v1/adoption-announcements/{announcementId.Value}", UriKind.Relative));

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<AdoptionAnnouncementDetailsResponse>(stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize AdoptionAnnouncementDetailsResponse");
    }

    public static async Task<PaginationResponse<AdoptionAnnouncementListItemResponse>> GetAllAsync(TestApiClient apiClient)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.GetAsync(new Uri("api/v1/adoption-announcements", UriKind.Relative));

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<PaginationResponse<AdoptionAnnouncementListItemResponse>>(
                   stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize PaginationResponse");
    }
}

using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

internal static class CleanerService
{
    public static async Task CleanDatabaseAsync(TestApiClient apiClient)
    {
        await DeleteAllAdoptionAnnouncementsAsync(apiClient);
        await DeleteAllCatsAsync(apiClient);
        await DeleteAllPersonsAsync(apiClient);
    }

    private static async Task DeleteAllAdoptionAnnouncementsAsync(TestApiClient apiClient)
    {
        PaginationResponse<AdoptionAnnouncementListItemResponse> announcementsResponse =
            await AdoptionAnnouncementApiQueryService.GetAllAsync(apiClient);

        foreach (AdoptionAnnouncementListItemResponse announcement in announcementsResponse.Items)
        {
            await apiClient.Http.DeleteAsync(new Uri($"api/v1/adoption-announcements/{announcement.Id.Value}", UriKind.Relative));
        }
    }

    private static async Task DeleteAllCatsAsync(TestApiClient apiClient)
    {
        PaginationResponse<CatListItemResponse> catsResponse =
            await CatApiQueryService.GetAllAsync(apiClient);

        foreach (CatListItemResponse cat in catsResponse.Items)
        {
            await apiClient.Http.DeleteAsync(new Uri($"api/v1/cats/{cat.Id.Value}", UriKind.Relative));
        }
    }

    private static async Task DeleteAllPersonsAsync(TestApiClient apiClient)
    {
        PaginationResponse<PersonListItemResponse> personsResponse =
            await PersonApiQueryService.GetAllAsync(apiClient);

        foreach (PersonListItemResponse person in personsResponse.Items)
        {
            await apiClient.Http.DeleteAsync(new Uri($"api/v1/persons/{person.Id}", UriKind.Relative));
        }
    }
}

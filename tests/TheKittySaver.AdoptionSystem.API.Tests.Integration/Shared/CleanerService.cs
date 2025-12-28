using System.Text.Json;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;

// Shared/CleanerService.cs
namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

internal static class CleanerService
{
    public static async Task CleanDatabaseAsync(TestApiClient apiClient)
    {
        PaginationResponse<PersonListItemResponse> personsResponse =
            await PersonApiQueryService.GetAllAsync(apiClient);

        foreach (PersonListItemResponse person in personsResponse.Items)
        {
            await apiClient.Http.DeleteAsync(new Uri($"api/v1/persons/{person.Id}", UriKind.Relative));
        }
    }
}

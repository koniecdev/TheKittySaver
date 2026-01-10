using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Persons;

internal static class PersonApiQueryService
{
    public static async Task<PersonDetailsResponse> GetByIdAsync(TestApiClient apiClient, PersonId personId)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.GetAsync(new Uri($"api/v1/persons/{personId.Value}", UriKind.Relative));

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<PersonDetailsResponse>(stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize PersonDetailsResponse");
    }

    public static async Task<PaginationResponse<PersonListItemResponse>> GetAllAsync(TestApiClient apiClient)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.GetAsync(new Uri("api/v1/persons", UriKind.Relative));

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<PaginationResponse<PersonListItemResponse>>(
                   stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize PaginationResponse");
    }
}

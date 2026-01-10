using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Persons;

internal static class PersonAddressApiQueryService
{
    public static async Task<PersonAddressResponse> GetByIdAsync(
        TestApiClient apiClient,
        PersonId personId,
        AddressId addressId)
    {
        HttpResponseMessage httpResponseMessage =
            await apiClient.Http.GetAsync(new Uri($"api/v1/persons/{personId.Value}/addresses/{addressId.Value}", UriKind.Relative));

        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        return JsonSerializer.Deserialize<PersonAddressResponse>(stringResponse, apiClient.JsonOptions)
               ?? throw new JsonException("Failed to deserialize PersonAddressResponse");
    }
}

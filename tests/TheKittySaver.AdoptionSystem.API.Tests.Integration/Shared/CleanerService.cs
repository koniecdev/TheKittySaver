using System.Text.Json;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;

public static class CleanerService
{
    public static async Task CleanDatabaseAsync(HttpClient client, JsonSerializerOptions jsonSerializerOptions)
    {
        HttpResponseMessage httpResponseMessage = 
            await client.GetAsync(new Uri("/api/v1/persons", UriKind.Relative));
        
        string content = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        
        PaginationResponse<PersonListItemResponse> personsResponse =
            JsonSerializer.Deserialize<PaginationResponse<PersonListItemResponse>>(content, jsonSerializerOptions)
            ?? throw new JsonException("Could not deserialize the response");

        foreach (PersonListItemResponse person in personsResponse.Items)
        {
            await client.DeleteAsync(new Uri($"api/v1/persons/{person.Id}", UriKind.Relative));
        }
    }
}

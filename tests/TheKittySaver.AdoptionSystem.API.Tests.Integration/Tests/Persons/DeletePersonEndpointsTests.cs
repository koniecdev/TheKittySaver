using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

[Collection("Api")]
public sealed class DeletePersonEndpointsTests : AsyncLifetimeTestBase
{
    protected override HttpClient HttpClient { get; }
    protected override JsonSerializerOptions JsonSerializerOptions { get; }
    
    public DeletePersonEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        HttpClient = appFactory.CreateClient();
        JsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptionsSnapshot<JsonOptions>>().Value.SerializerOptions;
    }
    
    //todo: testy sprawdzające anonimizacje danych w przypadku usunięcia konta, które ma istniejące koty/ogłoszenia 

    [Fact]
    public async Task DeletePerson_ShouldReturnNoContent_WhenExistingUserIdIsProvided()
    {
        // Arrange
        PersonId personId = 
            await PersonApiFactory.CreateRandomAndGetIdAsync(HttpClient, JsonSerializerOptions, Faker);

        // Act
        HttpResponseMessage httpResponseMessage =
            await HttpClient.DeleteAsync(new Uri($"api/v1/persons/{personId}", UriKind.Relative));

        // Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getDeletedHttpResponseMessage = 
            await HttpClient.DeleteAsync(new Uri($"api/v1/persons/{personId}", UriKind.Relative));
        getDeletedHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task DeletePerson_ShouldReturnNotFound_WhenNonExistingUserIdIsProvided()
    {
        // Act
        HttpResponseMessage httpResponseMessage =
            await HttpClient.DeleteAsync(new Uri($"api/v1/persons/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await httpResponseMessage.ToProblemDetailsAsync();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

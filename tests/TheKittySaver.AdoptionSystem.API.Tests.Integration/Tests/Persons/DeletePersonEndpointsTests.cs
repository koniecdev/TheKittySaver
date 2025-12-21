using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

public sealed class DeletePersonEndpointsTests(TheKittySaverApiFactory appFactory)
    : PersonEndpointsTestBase(appFactory)
{
    //todo: testy sprawdzające anonimizacje danych w przypadku usunięcia konta, które ma istniejące koty/ogłoszenia

    [Fact]
    public async Task DeletePerson_ShouldReturnNoContent_WhenExistingUserIdIsProvided()
    {
        // Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);

        // Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/persons/{personId}", UriKind.Relative));

        // Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getDeletedHttpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/persons/{personId}", UriKind.Relative));
        getDeletedHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePerson_ShouldReturnNotFound_WhenNonExistingUserIdIsProvided()
    {
        // Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/persons/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await httpResponseMessage.ToProblemDetailsAsync();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

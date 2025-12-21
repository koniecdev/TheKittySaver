using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

public sealed class DeleteCatEndpointsTests(TheKittySaverApiFactory appFactory)
    : CatEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task DeleteCat_ShouldReturnNoContent_WhenExistingCatIdIsProvided()
    {
        // Arrange
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestPersonId);

        // Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/cats/{catId}", UriKind.Relative));

        // Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getDeletedHttpResponseMessage =
            await ApiClient.Http.GetAsync(new Uri($"api/v1/cats/{catId}", UriKind.Relative));
        getDeletedHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCat_ShouldReturnNotFound_WhenNonExistingCatIdIsProvided()
    {
        // Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/cats/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await httpResponseMessage.ToProblemDetailsAsync();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

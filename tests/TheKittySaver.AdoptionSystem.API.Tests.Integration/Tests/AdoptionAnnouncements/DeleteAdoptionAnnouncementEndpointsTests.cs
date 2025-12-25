using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

public sealed class DeleteAdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory)
    : AdoptionAnnouncementEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task DeleteAdoptionAnnouncement_ShouldReturnNoContent_WhenExistingAnnouncementIdIsProvided()
    {
        //Arrange
        AdoptionAnnouncementId announcementId = await AdoptionAnnouncementApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, TestCatId);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/adoption-announcements/{announcementId}", UriKind.Relative));

        //Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getDeletedHttpResponseMessage =
            await ApiClient.Http.GetAsync(new Uri($"api/v1/adoption-announcements/{announcementId}", UriKind.Relative));
        getDeletedHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAdoptionAnnouncement_ShouldReturnNotFound_WhenNonExistingAnnouncementIdIsProvided()
    {
        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/adoption-announcements/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await httpResponseMessage.ToProblemDetailsAsync();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

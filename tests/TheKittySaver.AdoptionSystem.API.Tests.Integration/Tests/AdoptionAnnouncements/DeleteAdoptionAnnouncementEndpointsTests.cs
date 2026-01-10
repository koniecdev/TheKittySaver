using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

public sealed class DeleteAdoptionAnnouncementEndpointsTests : EndpointsTestBase
{
    public DeleteAdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task DeleteAdoptionAnnouncement_ShouldReturnNoContent_WhenExistingAnnouncementIdIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        CreateAdoptionAnnouncementRequest request = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [catId]);
        AdoptionAnnouncementId aaId = await AdoptionAnnouncementApiFactory.CreateAndGetIdAsync(ApiClient, request);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.DeleteAsync(new Uri($"api/v1/adoption-announcements/{aaId}", UriKind.Relative));

        //Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getDeletedHttpResponseMessage =
            await ApiClient.Http.GetAsync(new Uri($"api/v1/adoption-announcements/{aaId}", UriKind.Relative));
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

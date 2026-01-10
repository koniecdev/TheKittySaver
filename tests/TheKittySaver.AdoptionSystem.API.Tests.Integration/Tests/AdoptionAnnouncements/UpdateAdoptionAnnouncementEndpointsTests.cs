using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

public sealed class UpdateAdoptionAnnouncementEndpointsTests : EndpointsTestBase
{
    public UpdateAdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task UpdateAdoptionAnnouncement_ShouldMapEveryRequestProperty_WhenValidDataIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        AdoptionAnnouncementDetailsResponse aa = 
            await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, [catId]);
        
        UpdateAdoptionAnnouncementRequest updateAaRequest = 
            AdoptionAnnouncementApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/adoption-announcements/{aa.Id}", UriKind.Relative), updateAaRequest);

        //Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        AdoptionAnnouncementDetailsResponse response =
            await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, aa.Id);
        response.ShouldNotBeNull();
        response.Id.ShouldBe(aa.Id);
        response.PersonId.ShouldBe(personId);
        response.Title.ShouldNotBeEmpty();
        response.Username.ShouldNotBeEmpty();
        response.Description.ShouldBe(updateAaRequest.Description);
        response.AddressPostalCode.ShouldBe(updateAaRequest.AddressPostalCode);
        response.AddressRegion.ShouldBe(updateAaRequest.AddressRegion);
        response.AddressCity.ShouldBe(updateAaRequest.AddressCity);
        response.AddressLine.ShouldBe(updateAaRequest.AddressLine);
        response.Email.ShouldBe(updateAaRequest.Email);
        response.PhoneNumber.ShouldBe(updateAaRequest.PhoneNumber);
        response.Status.ShouldBe(AnnouncementStatusType.Active);
    }

    [Fact]
    public async Task UpdateAdoptionAnnouncement_ShouldReturnNotFound_WhenNotExistingAnnouncementIdIsProvided()
    {
        //Arrange
        UpdateAdoptionAnnouncementRequest request = AdoptionAnnouncementApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/adoption-announcements/{Guid.NewGuid()}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

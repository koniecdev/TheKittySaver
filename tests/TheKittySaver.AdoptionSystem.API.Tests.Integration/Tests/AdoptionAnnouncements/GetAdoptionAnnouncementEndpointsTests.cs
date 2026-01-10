using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.AdoptionAnnouncements;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Cats;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.AdoptionAnnouncements;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

public sealed class GetAdoptionAnnouncementEndpointsTests : EndpointsTestBase
{
    public GetAdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldReturnNotFound_WhenRandomIdIsProvided()
    {
        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.GetAsync(new Uri($"/api/v1/adoption-announcements/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldMapAllProperties_WhenAnnouncementExists()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        CreateAdoptionAnnouncementRequest createAaRequest = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [catId]);
        AdoptionAnnouncementId aaId = await AdoptionAnnouncementApiFactory.CreateAndGetIdAsync(ApiClient, createAaRequest);

        //Act
        AdoptionAnnouncementDetailsResponse response =
            await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, aaId);

        //Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(aaId);
        response.PersonId.ShouldBe(personId);
        response.Title.ShouldNotBeEmpty();
        response.Username.ShouldNotBeEmpty();
        response.Description.ShouldBe(createAaRequest.Description);
        response.AddressPostalCode.ShouldBe(createAaRequest.AddressPostalCode);
        response.AddressRegion.ShouldBe(createAaRequest.AddressRegion);
        response.AddressCity.ShouldBe(createAaRequest.AddressCity);
        response.AddressLine.ShouldBe(createAaRequest.AddressLine);
        response.Email.ShouldBe(createAaRequest.Email);
        response.PhoneNumber.ShouldBe(createAaRequest.PhoneNumber);
        response.Status.ShouldBe(AnnouncementStatusType.Active);
    }

    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldReturnProperAnnouncement_WhenMultipleAnnouncementsExist()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        CreateAdoptionAnnouncementRequest request = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [catId]);
        AdoptionAnnouncementId aaId = await AdoptionAnnouncementApiFactory.CreateAndGetIdAsync(ApiClient, request);
        
        PersonId anotherPersonId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId anotherCatId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, anotherPersonId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, anotherCatId);
        CreateAdoptionAnnouncementRequest anotherAaRequest = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [anotherCatId]);
        _ = await AdoptionAnnouncementApiFactory.CreateAndGetIdAsync(ApiClient, anotherAaRequest);

        //Act
        AdoptionAnnouncementDetailsResponse response =
            await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, aaId);

        //Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(aaId);
    }
}

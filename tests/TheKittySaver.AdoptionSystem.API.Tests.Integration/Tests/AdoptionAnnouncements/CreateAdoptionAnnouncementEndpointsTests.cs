using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

public sealed class CreateAdoptionAnnouncementEndpointsTests : EndpointsTestBase
{
    public CreateAdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnAnnouncementId_WhenOneCatThatIsValidIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        
        CreateAdoptionAnnouncementRequest request = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [catId.Value]);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PostAsJsonAsync(new Uri("api/v1/adoption-announcements", UriKind.Relative), request);

        //Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        AdoptionAnnouncementId announcementId = 
            JsonSerializer.Deserialize<AdoptionAnnouncementId>(stringResponse, ApiClient.JsonOptions);
        announcementId.Value.ShouldNotBe(Guid.Empty);

        AdoptionAnnouncementDetailsResponse announcementResponse =
            await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, announcementId);
        announcementResponse.ShouldNotBeNull();
        announcementResponse.Description.ShouldBe(request.Description);
        announcementResponse.AddressCity.ShouldBe(request.AddressCity);
        announcementResponse.Email.ShouldBe(request.Email);
        announcementResponse.PhoneNumber.ShouldBe(request.PhoneNumber);
    }
    
    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnAnnouncementId_WhenMultipleCompatibleCatsAreProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreateCatRequest catRequest = CatApiFactory.GenerateRandomCreateRequest(Faker, personId, isHealthy: true);
        CreateCatRequest anotherCatRequest = CatApiFactory.GenerateRandomCreateRequest(Faker, personId, isHealthy: true);
        CatId catId = await CatApiFactory.CreateAndGetIdAsync(ApiClient, catRequest);
        CatId anotherCatId = await CatApiFactory.CreateAndGetIdAsync(ApiClient, anotherCatRequest);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, anotherCatId);
        
        CreateAdoptionAnnouncementRequest createAaRequest = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [catId.Value, anotherCatId.Value]);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
            new Uri("api/v1/adoption-announcements", UriKind.Relative), createAaRequest);

        //Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        AdoptionAnnouncementId announcementId = 
            JsonSerializer.Deserialize<AdoptionAnnouncementId>(stringResponse, ApiClient.JsonOptions);
        announcementId.Value.ShouldNotBe(Guid.Empty);

        AdoptionAnnouncementDetailsResponse announcementResponse =
            await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, announcementId);
        announcementResponse.ShouldNotBeNull();
        announcementResponse.Description.ShouldBe(createAaRequest.Description);
        announcementResponse.AddressCity.ShouldBe(createAaRequest.AddressCity);
        announcementResponse.Email.ShouldBe(createAaRequest.Email);
        announcementResponse.PhoneNumber.ShouldBe(createAaRequest.PhoneNumber);
        string[] catNames = [catRequest.Name, anotherCatRequest.Name];
        catNames = catNames.OrderBy(x => x).ToArray();
        announcementResponse.Title.ShouldBe($"{catNames[0]}, {catNames[1]}");
    }
    
    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnBadRequest_WhenMultipleUncompatibleCatsAreProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreateCatRequest catRequest = CatApiFactory.GenerateRandomCreateRequest(Faker, personId, isHealthy: true);
        CreateCatRequest sickCatRequest = CatApiFactory.GenerateRandomCreateRequest(Faker, personId, isHealthy: false);
        CatId catId = await CatApiFactory.CreateAndGetIdAsync(ApiClient, catRequest);
        CatId sickCatId = await CatApiFactory.CreateAndGetIdAsync(ApiClient, sickCatRequest);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, sickCatId);
        
        CreateAdoptionAnnouncementRequest createAaRequest = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [catId.Value, sickCatId.Value]);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
            new Uri("api/v1/adoption-announcements", UriKind.Relative), createAaRequest);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreateAdoptionAnnouncement_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        //Arrange
        CatId nonExistentCatId = CatId.Create();
        CreateAdoptionAnnouncementRequest request = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [nonExistentCatId.Value]);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PostAsJsonAsync(new Uri("api/v1/adoption-announcements", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

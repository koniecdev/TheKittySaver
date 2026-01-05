using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
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
        
        CreateAdoptionAnnouncementRequest request = AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, TestCatId);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PostAsJsonAsync(new Uri("api/v1/adoption-announcements", UriKind.Relative), request);

        //Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        AdoptionAnnouncementId announcementId = JsonSerializer.Deserialize<AdoptionAnnouncementId>(stringResponse, ApiClient.JsonOptions);
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
    public async Task CreateAdoptionAnnouncement_ShouldReturnNotFound_WhenCatDoesNotExist()
    {
        //Arrange
        CatId nonExistentCatId = new(Guid.NewGuid());
        CreateAdoptionAnnouncementRequest request = AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, nonExistentCatId);

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

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

public sealed class GetAdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory)
    : AdoptionAnnouncementEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldReturnNotFound_WhenRandomIdIsProvided()
    {
        // Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.GetAsync(new Uri($"/api/v1/adoption-announcements/{Guid.NewGuid()}", UriKind.Relative));

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldMapAllProperties_WhenAnnouncementExists()
    {
        // Arrange
        AdoptionAnnouncementDetailsResponse createdAnnouncement =
            await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, TestCatId);

        // Act
        AdoptionAnnouncementDetailsResponse response =
            await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, createdAnnouncement.Id);

        // Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(createdAnnouncement.Id);
        response.PersonId.ShouldBe(TestPersonId);
        response.Description.ShouldBe(createdAnnouncement.Description);
        response.AddressCity.ShouldBe(createdAnnouncement.AddressCity);
        response.Email.ShouldBe(createdAnnouncement.Email);
        response.PhoneNumber.ShouldBe(createdAnnouncement.PhoneNumber);
    }

    [Fact]
    public async Task GetAdoptionAnnouncement_ShouldReturnProperAnnouncement_WhenMultipleAnnouncementsExist()
    {
        // Arrange
        AdoptionAnnouncementDetailsResponse announcement =
            await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, TestCatId);

        CatId anotherCatId = await CatApiFactory.CreateRandomWithThumbnailAndGetIdAsync(ApiClient, Faker, TestPersonId);
        _ = await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, anotherCatId);

        // Act
        AdoptionAnnouncementDetailsResponse response =
            await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, announcement.Id);

        // Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(announcement.Id);
    }
}

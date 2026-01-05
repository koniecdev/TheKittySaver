// using System.Net;
// using System.Net.Http.Json;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Shouldly;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;
//
// public sealed class UpdateAdoptionAnnouncementEndpointsTests(TheKittySaverApiFactory appFactory)
//     : EndpointsTestBase(appFactory)
// {
//     [Fact]
//     public async Task UpdateAdoptionAnnouncement_ShouldMapEveryRequestProperty_WhenValidDataIsProvided()
//     {
//         //Arrange
//         AdoptionAnnouncementDetailsResponse announcement =
//             await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, TestCatId);
//         UpdateAdoptionAnnouncementRequest request = AdoptionAnnouncementApiFactory.GenerateRandomUpdateRequest(Faker);
//
//         //Act
//         HttpResponseMessage httpResponseMessage =
//             await ApiClient.Http.PutAsJsonAsync(
//                 new Uri($"api/v1/adoption-announcements/{announcement.Id}", UriKind.Relative), request);
//
//         //Assert
//         _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
//         httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);
//
//         AdoptionAnnouncementDetailsResponse announcementResponse =
//             await AdoptionAnnouncementApiQueryService.GetByIdAsync(ApiClient, announcement.Id);
//         announcementResponse.ShouldNotBeNull();
//         announcementResponse.Description.ShouldBe(request.Description);
//         announcementResponse.AddressCity.ShouldBe(request.AddressCity);
//         announcementResponse.AddressPostalCode.ShouldBe(request.AddressPostalCode);
//         announcementResponse.Email.ShouldBe(request.Email);
//         announcementResponse.PhoneNumber.ShouldBe(request.PhoneNumber);
//     }
//
//     [Fact]
//     public async Task UpdateAdoptionAnnouncement_ShouldReturnNotFound_WhenNotExistingAnnouncementIdIsProvided()
//     {
//         //Arrange
//         UpdateAdoptionAnnouncementRequest request = AdoptionAnnouncementApiFactory.GenerateRandomUpdateRequest(Faker);
//
//         //Act
//         HttpResponseMessage httpResponseMessage =
//             await ApiClient.Http.PutAsJsonAsync(
//                 new Uri($"api/v1/adoption-announcements/{Guid.NewGuid()}", UriKind.Relative), request);
//
//         //Assert
//         httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//         ProblemDetails? problemDetails =
//             await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
//         problemDetails.ShouldNotBeNull();
//         problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
//     }
// }

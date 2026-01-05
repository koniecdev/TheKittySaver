// using Shouldly;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
// using TheKittySaver.AdoptionSystem.Contracts.Common;
// using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;
//
// public sealed class GetAdoptionAnnouncementsEndpointsTests(TheKittySaverApiFactory appFactory)
//     : EndpointsTestBase(appFactory)
// {
//     [Fact]
//     public async Task GetAdoptionAnnouncements_ShouldReturnEmptyItemList_WhenNoAnnouncementsExist()
//     {
//         //Act
//         PaginationResponse<AdoptionAnnouncementListItemResponse> response =
//             await AdoptionAnnouncementApiQueryService.GetAllAsync(ApiClient);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Items.ShouldNotBeNull();
//         response.Items.Count.ShouldBe(0);
//     }
//
//     [Fact]
//     public async Task GetAdoptionAnnouncements_ShouldMapAllProperties_WhenAnnouncementExists()
//     {
//         //Arrange
//         AdoptionAnnouncementDetailsResponse announcementResponse =
//             await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, TestCatId);
//
//         //Act
//         PaginationResponse<AdoptionAnnouncementListItemResponse> response =
//             await AdoptionAnnouncementApiQueryService.GetAllAsync(ApiClient);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Items.ShouldNotBeNull();
//         response.Items.Count.ShouldBe(1);
//
//         AdoptionAnnouncementListItemResponse announcementFromList = response.Items.First();
//         announcementFromList.Id.ShouldBe(announcementResponse.Id);
//         announcementFromList.PersonId.ShouldBe(announcementResponse.PersonId);
//         announcementFromList.Description.ShouldBe(announcementResponse.Description);
//         announcementFromList.AddressCity.ShouldBe(announcementResponse.AddressCity);
//         announcementFromList.Email.ShouldBe(announcementResponse.Email);
//     }
//
//     [Fact]
//     public async Task GetAdoptionAnnouncements_ShouldReturnMultipleAnnouncements_WhenMultipleAnnouncementsExist()
//     {
//         //Arrange
//         _ = await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, TestCatId);
//
//         CatId anotherCatId = await CatApiFactory.CreateRandomWithThumbnailAndGetIdAsync(ApiClient, Faker, TestPersonId);
//         _ = await AdoptionAnnouncementApiFactory.CreateRandomAsync(ApiClient, Faker, anotherCatId);
//
//         //Act
//         PaginationResponse<AdoptionAnnouncementListItemResponse> response =
//             await AdoptionAnnouncementApiQueryService.GetAllAsync(ApiClient);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Items.ShouldNotBeNull();
//         response.Items.Count.ShouldBe(2);
//     }
// }

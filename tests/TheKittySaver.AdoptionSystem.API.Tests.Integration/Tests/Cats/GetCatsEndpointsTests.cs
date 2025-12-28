// using Shouldly;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
// using TheKittySaver.AdoptionSystem.Contracts.Common;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;
//
// public sealed class GetCatsEndpointsTests(TheKittySaverApiFactory appFactory)
//     : CatEndpointsTestBase(appFactory)
// {
//     [Fact]
//     public async Task GetCats_ShouldReturnEmptyItemList_WhenNoCatsExist()
//     {
//         //Act
//         PaginationResponse<CatListItemResponse> response = await CatApiQueryService.GetAllAsync(ApiClient);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Items.ShouldNotBeNull();
//         response.Items.Count.ShouldBe(0);
//     }
//
//     [Fact]
//     public async Task GetCats_ShouldMapAllOfCatProperties_WhenCatExists()
//     {
//         //Arrange
//         CatDetailsResponse catResponse = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, TestPersonId);
//
//         //Act
//         PaginationResponse<CatListItemResponse> response = await CatApiQueryService.GetAllAsync(ApiClient);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Items.ShouldNotBeNull();
//         response.Items.Count.ShouldBe(1);
//
//         CatListItemResponse catFromList = response.Items.First();
//         catFromList.Id.ShouldBe(catResponse.Id);
//         catFromList.PersonId.ShouldBe(catResponse.PersonId);
//         catFromList.Name.ShouldBe(catResponse.Name);
//         catFromList.Description.ShouldBe(catResponse.Description);
//         catFromList.Age.ShouldBe(catResponse.Age);
//     }
//
//     [Fact]
//     public async Task GetCats_ShouldReturnMultipleCats_WhenMultipleCatsExist()
//     {
//         //Arrange
//         _ = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, TestPersonId);
//         _ = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, TestPersonId);
//
//         //Act
//         PaginationResponse<CatListItemResponse> response = await CatApiQueryService.GetAllAsync(ApiClient);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Items.ShouldNotBeNull();
//         response.Items.Count.ShouldBe(2);
//     }
// }

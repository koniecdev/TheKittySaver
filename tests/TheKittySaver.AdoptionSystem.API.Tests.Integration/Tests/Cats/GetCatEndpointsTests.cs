// using System.Net;
// using System.Net.Http.Json;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Shouldly;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;
//
// public sealed class GetCatEndpointsTests(TheKittySaverApiFactory appFactory)
//     : CatEndpointsTestBase(appFactory)
// {
//     [Fact]
//     public async Task GetCat_ShouldReturnNotFound_WhenRandomIdIsProvided()
//     {
//         //Act
//         HttpResponseMessage httpResponseMessage =
//             await ApiClient.Http.GetAsync(new Uri($"/api/v1/cats/{Guid.NewGuid()}", UriKind.Relative));
//
//         //Assert
//         httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//
//         ProblemDetails? problemDetails =
//             await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
//         problemDetails.ShouldNotBeNull();
//         problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
//     }
//
//     [Fact]
//     public async Task GetCat_ShouldMapAllOfCatProperties_WhenCatExists()
//     {
//         //Arrange
//         CatDetailsResponse createdCat = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, TestPersonId);
//
//         //Act
//         CatDetailsResponse response = await CatApiQueryService.GetByIdAsync(ApiClient, createdCat.Id);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Id.ShouldBe(createdCat.Id);
//         response.PersonId.ShouldBe(TestPersonId);
//         response.Name.ShouldBe(createdCat.Name);
//         response.Description.ShouldBe(createdCat.Description);
//         response.Age.ShouldBe(createdCat.Age);
//         response.Vaccinations.ShouldNotBeNull();
//         response.Vaccinations.Count.ShouldBe(0);
//         response.GalleryItems.ShouldNotBeNull();
//         response.GalleryItems.Count.ShouldBe(0);
//     }
//
//     [Fact]
//     public async Task GetCat_ShouldReturnProperCat_WhenMultipleCatsExist()
//     {
//         //Arrange
//         CatDetailsResponse cat = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, TestPersonId);
//         _ = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, TestPersonId);
//
//         //Act
//         CatDetailsResponse response = await CatApiQueryService.GetByIdAsync(ApiClient, cat.Id);
//
//         //Assert
//         response.ShouldNotBeNull();
//         response.Id.ShouldBe(cat.Id);
//     }
// }

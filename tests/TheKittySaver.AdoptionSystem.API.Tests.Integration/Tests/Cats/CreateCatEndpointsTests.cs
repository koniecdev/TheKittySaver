// using System.Net;
// using System.Net.Http.Json;
// using System.Text.Json;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Shouldly;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
// using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
// using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;
//
// public sealed class CreateCatEndpointsTests(TheKittySaverApiFactory appFactory)
//     : CatEndpointsTestBase(appFactory)
// {
//     [Fact]
//     public async Task CreateCat_ShouldReturnCatId_WhenValidDataIsProvided()
//     {
//         //Arrange
//         CreateCatRequest request = CatApiFactory.GenerateRandomCreateRequest(Faker, TestPersonId);
//
//         //Act
//         HttpResponseMessage httpResponseMessage =
//             await ApiClient.Http.PostAsJsonAsync(new Uri("api/v1/cats", UriKind.Relative), request);
//
//         //Assert
//         string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
//         httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
//
//         CatId catId = JsonSerializer.Deserialize<CatId>(stringResponse, ApiClient.JsonOptions);
//         catId.Value.ShouldNotBe(Guid.Empty);
//
//         CatDetailsResponse catResponse = await CatApiQueryService.GetByIdAsync(ApiClient, catId);
//         catResponse.ShouldNotBeNull();
//         catResponse.Name.ShouldBe(request.Name);
//         catResponse.Description.ShouldBe(request.Description);
//         catResponse.Age.ShouldBe(request.Age);
//         catResponse.PersonId.ShouldBe(TestPersonId);
//     }
//
//     [Fact]
//     public async Task CreateCat_ShouldReturnNotFound_WhenPersonDoesNotExist()
//     {
//         //Arrange
//         PersonId nonExistentPersonId = new(Guid.NewGuid());
//         CreateCatRequest request = CatApiFactory.GenerateRandomCreateRequest(Faker, nonExistentPersonId);
//
//         //Act
//         HttpResponseMessage httpResponseMessage =
//             await ApiClient.Http.PostAsJsonAsync(new Uri("api/v1/cats", UriKind.Relative), request);
//
//         //Assert
//         httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
//         ProblemDetails? problemDetails =
//             await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
//         problemDetails.ShouldNotBeNull();
//         problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
//     }
// }

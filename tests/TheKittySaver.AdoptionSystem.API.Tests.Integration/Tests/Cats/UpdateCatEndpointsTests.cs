using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

public sealed class UpdateCatEndpointsTests(TheKittySaverApiFactory appFactory)
    : CatEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task UpdateCat_ShouldMapEveryRequestProperty_WhenValidDataIsProvided()
    {
        // Arrange
        CatDetailsResponse cat = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, TestPersonId);
        UpdateCatRequest request = CatApiFactory.GenerateRandomUpdateRequest(Faker);

        // Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/cats/{cat.Id}", UriKind.Relative), request);

        // Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        CatDetailsResponse catResponse = await CatApiQueryService.GetByIdAsync(ApiClient, cat.Id);
        catResponse.ShouldNotBeNull();
        catResponse.Name.ShouldBe(request.Name);
        catResponse.Description.ShouldBe(request.Description);
        catResponse.Age.ShouldBe(request.Age);
        catResponse.Gender.ShouldBe(request.Gender);
        catResponse.Color.ShouldBe(request.Color);
        catResponse.HealthStatus.ShouldBe(request.HealthStatus);
        catResponse.Temperament.ShouldBe(request.Temperament);
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenNotExistingCatIdIsProvided()
    {
        // Arrange
        UpdateCatRequest request = CatApiFactory.GenerateRandomUpdateRequest(Faker);

        // Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/cats/{Guid.NewGuid()}", UriKind.Relative), request);

        // Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

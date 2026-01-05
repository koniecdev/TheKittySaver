using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

public sealed class UpdateCatEndpointsTests : EndpointsTestBase
{
    public UpdateCatEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task UpdateCat_ShouldMapEveryRequestProperty_WhenValidDataIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        UpdateCatRequest updateCatRequest = CatApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/cats/{catId}", UriKind.Relative), updateCatRequest);

        //Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        CatDetailsResponse catResponse = await CatApiQueryService.GetByIdAsync(ApiClient, catId);
        catResponse.ShouldNotBeNull();
        catResponse.Id.ShouldBe(catId);
        catResponse.PersonId.ShouldBe(personId);
        catResponse.AdoptionAnnouncementId.ShouldBe(null);
        catResponse.Name.ShouldBe(updateCatRequest.Name);
        catResponse.Description.ShouldBe(updateCatRequest.Description);
        catResponse.Age.ShouldBe(updateCatRequest.Age);
        catResponse.Gender.ShouldBe(updateCatRequest.Gender);
        catResponse.Color.ShouldBe(updateCatRequest.Color);
        catResponse.WeightInGrams.ShouldBe(updateCatRequest.WeightInGrams);
        catResponse.HealthStatus.ShouldBe(updateCatRequest.HealthStatus);
        catResponse.SpecialNeedsDescription.ShouldBe(updateCatRequest.SpecialNeedsDescription);
        catResponse.SpecialNeedsSeverityType.ShouldBe(updateCatRequest.SpecialNeedsSeverityType);
        catResponse.Temperament.ShouldBe(updateCatRequest.Temperament);
        catResponse.AdoptionHistoryReturnCount.ShouldBe(updateCatRequest.AdoptionHistoryReturnCount);
        catResponse.AdoptionHistoryLastReturnDate.ShouldBe(updateCatRequest.AdoptionHistoryLastReturnDate);
        catResponse.AdoptionHistoryLastReturnDate.ShouldBe(updateCatRequest.AdoptionHistoryLastReturnDate);
        catResponse.AdoptionHistoryLastReturnReason.ShouldBe(updateCatRequest.AdoptionHistoryLastReturnReason);
        catResponse.IsNeutered.ShouldBe(updateCatRequest.IsNeutered);
        catResponse.FivStatus.ShouldBe(updateCatRequest.FivStatus);
        catResponse.FelvStatus.ShouldBe(updateCatRequest.FelvStatus);
        catResponse.InfectiousDiseaseStatusLastTestedAt.ShouldBe(updateCatRequest.InfectiousDiseaseStatusLastTestedAt);
        catResponse.Vaccinations.ShouldBeEmpty();
        catResponse.GalleryItems.ShouldBeEmpty();
    }

    [Fact]
    public async Task UpdateCat_ShouldReturnNotFound_WhenNotExistingCatIdIsProvided()
    {
        //Arrange
        UpdateCatRequest request = CatApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act 
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/cats/{Guid.NewGuid()}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
}

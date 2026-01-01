using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

public sealed class GetCatEndpointsTests(TheKittySaverApiFactory appFactory)
    : CatEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task GetCat_ShouldReturnNotFound_WhenRandomIdIsProvided()
    {
        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.GetAsync(new Uri($"/api/v1/cats/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetCat_ShouldMapAllOfCatProperties_WhenCatExists()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreateCatRequest createCatRequest = CatApiFactory.GenerateRandomCreateRequest(Faker, personId);
        CatId catId = await CatApiFactory.CreateAndGetIdAsync(ApiClient, createCatRequest);

        //Act
        CatDetailsResponse response = await CatApiQueryService.GetByIdAsync(ApiClient, catId);

        //Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(catId);
        response.PersonId.ShouldBe(personId);
        response.AdoptionAnnouncementId.ShouldBe(null);
        response.Name.ShouldBe(createCatRequest.Name);
        response.Description.ShouldBe(createCatRequest.Description);
        response.Age.ShouldBe(createCatRequest.Age);
        response.Gender.ShouldBe(createCatRequest.Gender);
        response.Color.ShouldBe(createCatRequest.Color);
        response.WeightValueInKilograms.ShouldBe(createCatRequest.WeightValueInKilograms);
        response.HealthStatus.ShouldBe(createCatRequest.HealthStatus);
        response.SpecialNeedsDescription.ShouldBe(createCatRequest.SpecialNeedsDescription);
        response.SpecialNeedsSeverityType.ShouldBe(createCatRequest.SpecialNeedsSeverityType);
        response.Temperament.ShouldBe(createCatRequest.Temperament);
        response.AdoptionHistoryReturnCount.ShouldBe(createCatRequest.AdoptionHistoryReturnCount);
        response.AdoptionHistoryLastReturnDate.ShouldBe(createCatRequest.AdoptionHistoryLastReturnDate);
        response.AdoptionHistoryLastReturnDate.ShouldBe(createCatRequest.AdoptionHistoryLastReturnDate);
        response.AdoptionHistoryLastReturnReason.ShouldBe(createCatRequest.AdoptionHistoryLastReturnReason);
        response.IsNeutered.ShouldBe(createCatRequest.IsNeutered);
        response.FivStatus.ShouldBe(createCatRequest.FivStatus);
        response.FelvStatus.ShouldBe(createCatRequest.FelvStatus);
        response.InfectiousDiseaseStatusLastTestedAt.ShouldBe(createCatRequest.InfectiousDiseaseStatusLastTestedAt);
        response.Vaccinations.ShouldBeEmpty();
        response.GalleryItems.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCat_ShouldReturnProperCat_WhenMultipleCatsExist()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatDetailsResponse cat = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, personId);
        _ = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, personId);

        //Act
        CatDetailsResponse response = await CatApiQueryService.GetByIdAsync(ApiClient, cat.Id);

        //Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(cat.Id);
    }
}

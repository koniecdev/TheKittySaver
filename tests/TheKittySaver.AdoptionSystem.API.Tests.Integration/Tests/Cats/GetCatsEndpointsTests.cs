using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

public sealed class GetCatsEndpointsTests(TheKittySaverApiFactory appFactory)
    : CatEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task GetCats_ShouldReturnEmptyItemList_WhenNoCatsExist()
    {
        //Act
        PaginationResponse<CatListItemResponse> response = await CatApiQueryService.GetAllAsync(ApiClient);

        //Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetCats_ShouldMapAllOfCatProperties_WhenCatExists()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatDetailsResponse catDetailsResponse = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, personId);

        //Act
        PaginationResponse<CatListItemResponse> response = await CatApiQueryService.GetAllAsync(ApiClient);

        //Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(1);

        CatListItemResponse catFromList = response.Items.First();
        catFromList.Id.ShouldBe(catDetailsResponse.Id);
        catFromList.PersonId.ShouldBe(personId);
        catFromList.AdoptionAnnouncementId.ShouldBe(null);
        catFromList.Name.ShouldBe(catDetailsResponse.Name);
        catFromList.FivStatus.ShouldBe(catDetailsResponse.FivStatus);
        catFromList.FelvStatus.ShouldBe(catDetailsResponse.FelvStatus);
    }

    [Fact]
    public async Task GetCats_ShouldReturnMultipleCats_WhenMultipleCatsExist()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        _ = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, personId);
        _ = await CatApiFactory.CreateRandomAsync(ApiClient, Faker, personId);

        //Act
        PaginationResponse<CatListItemResponse> response = await CatApiQueryService.GetAllAsync(ApiClient);

        //Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(2);
    }
}

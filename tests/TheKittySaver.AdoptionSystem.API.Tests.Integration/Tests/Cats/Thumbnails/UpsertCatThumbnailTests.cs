using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats.Thumbnails;

public sealed class UpsertCatThumbnailTests(TheKittySaverApiFactory appFactory)
    : CatEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task UpsertCat_ShouldBeSuccessful_WhenValidDataIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        
        //Act
        CatThumbnailResponse response = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        
        //Assert
        response.ShouldNotBeNull();
        response.Id.Value.ShouldNotBe(Guid.Empty);
        response.CatId.ShouldBe(catId);
    }
}

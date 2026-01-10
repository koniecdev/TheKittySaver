using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Cats;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Cats;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Cats;

public sealed class CreateCatEndpointsTests : EndpointsTestBase
{
    public CreateCatEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task CreateCat_ShouldBeSuccessful_WhenValidDataIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreateCatRequest createCatRequest = CatApiFactory.GenerateRandomCreateRequest(Faker, personId);
        
        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
            new Uri("api/v1/cats", UriKind.Relative), createCatRequest);
        
        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        CatId catId = JsonSerializer.Deserialize<CatId>(stringResponse, ApiClient.JsonOptions);
        catId.Value.ShouldNotBe(Guid.Empty);
        
        CatDetailsResponse catDetailsResponse = await CatApiQueryService.GetByIdAsync(ApiClient, catId);
        catDetailsResponse.ShouldNotBeNull();
        catDetailsResponse.PersonId.ShouldBe(personId);
        catDetailsResponse.Name.ShouldBe(createCatRequest.Name);
        catDetailsResponse.AdoptionAnnouncementId.ShouldBe(null);
        catDetailsResponse.AdoptionHistoryLastReturnDate.ShouldBe(createCatRequest.AdoptionHistoryLastReturnDate);
    }
}

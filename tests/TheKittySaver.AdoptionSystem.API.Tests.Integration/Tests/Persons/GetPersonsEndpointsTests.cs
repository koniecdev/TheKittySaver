// Tests/Persons/GetPersonsEndpointsTests.cs

using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

public sealed class GetPersonsEndpointsTests(TheKittySaverApiFactory appFactory) : PersonEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task GetPersons_ShouldReturnEmptyItemList_WhenNoPersonsExists()
    {
        // Act
        PaginationResponse<PersonListItemResponse> response = 
            await PersonApiQueryService.GetAllAsync(ApiClient);

        // Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetPersons_ShouldMapAllOfPersonProperties_WhenPersonsExists()
    {
        // Arrange
        PersonDetailsResponse personResponse = await PersonApiFactory.CreateRandomAsync(ApiClient, Faker);

        // Act
        PaginationResponse<PersonListItemResponse> response = 
            await PersonApiQueryService.GetAllAsync(ApiClient);

        // Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(1);

        PersonListItemResponse personFromList = response.Items.First();
        personFromList.Id.ShouldBe(personResponse.Id);
        personFromList.Email.ShouldBe(personResponse.Email);
        personFromList.PhoneNumber.ShouldBe(personResponse.PhoneNumber);
        personFromList.Username.ShouldBe(personResponse.Username);
    }

    [Fact]
    public async Task GetPersons_ShouldReturnMultiplePersons_WhenMultiplePersonsExists()
    {
        // Arrange
        _ = await PersonApiFactory.CreateRandomAsync(ApiClient, Faker);
        _ = await PersonApiFactory.CreateRandomAsync(ApiClient, Faker);

        // Act
        PaginationResponse<PersonListItemResponse> response = 
            await PersonApiQueryService.GetAllAsync(ApiClient);

        // Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(2);
    }
}

using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Persons;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons.Addresses;

public sealed class CreatePersonAddressEndpointsTests : EndpointsTestBase
{
    public CreatePersonAddressEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task CreatePersonAddress_ShouldBeSuccessful_WhenValidDataIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreatePersonAddressRequest createPersonAddressRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses", UriKind.Relative), createPersonAddressRequest);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();

        AddressId addressId = JsonSerializer.Deserialize<AddressId>(stringResponse, ApiClient.JsonOptions);
        addressId.Value.ShouldNotBe(Guid.Empty);

        PersonAddressResponse personAddressResponse = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, addressId);
        personAddressResponse.ShouldNotBeNull();
        personAddressResponse.PersonId.ShouldBe(personId);
        personAddressResponse.Name.ShouldBe(createPersonAddressRequest.Name);
    }
}

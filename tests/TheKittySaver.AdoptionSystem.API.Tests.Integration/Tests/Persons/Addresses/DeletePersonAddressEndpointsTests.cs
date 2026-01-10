using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Persons;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons.Addresses;

public sealed class DeletePersonAddressEndpointsTests : EndpointsTestBase
{
    public DeletePersonAddressEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldReturnNoContent_WhenExistingAddressIdIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        AddressId addressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.DeleteAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{addressId.Value}", UriKind.Relative));

        //Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getDeletedHttpResponseMessage = await ApiClient.Http.GetAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{addressId.Value}", UriKind.Relative));
        getDeletedHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldReturnNotFound_WhenNonExistingAddressIdIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.DeleteAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await httpResponseMessage.ToProblemDetailsAsync();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldReturnNotFound_WhenNonExistingPersonIdIsProvided()
    {
        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.DeleteAsync(
            new Uri($"api/v1/persons/{Guid.NewGuid()}/addresses/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails problemDetails = await httpResponseMessage.ToProblemDetailsAsync();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldReturnNotFound_WhenAddressBelongsToDifferentPerson()
    {
        //Arrange
        PersonId firstPersonId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        PersonId secondPersonId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        AddressId addressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, firstPersonId);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.DeleteAsync(
            new Uri($"api/v1/persons/{secondPersonId.Value}/addresses/{addressId.Value}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldNotAffectOtherAddresses_WhenDeletingOne()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        AddressId firstAddressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        AddressId secondAddressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.DeleteAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{firstAddressId.Value}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage getDeletedHttpResponseMessage = await ApiClient.Http.GetAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{firstAddressId.Value}", UriKind.Relative));
        getDeletedHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        PersonAddressResponse secondAddressResponse = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, secondAddressId);
        secondAddressResponse.ShouldNotBeNull();
        secondAddressResponse.Id.ShouldBe(secondAddressId);
    }

    [Fact]
    public async Task DeletePersonAddress_ShouldUpdatePersonAddressesList_WhenAddressIsDeleted()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        AddressId addressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);

        PersonDetailsResponse personBeforeDelete = await PersonApiQueryService.GetByIdAsync(ApiClient, personId);
        personBeforeDelete.Addresses.Count.ShouldBe(1);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.DeleteAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{addressId.Value}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        PersonDetailsResponse personAfterDelete = await PersonApiQueryService.GetByIdAsync(ApiClient, personId);
        personAfterDelete.Addresses.Count.ShouldBe(0);
    }
}

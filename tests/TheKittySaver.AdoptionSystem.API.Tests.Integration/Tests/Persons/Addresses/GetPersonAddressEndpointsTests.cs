using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Persons;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons.Addresses;

public sealed class GetPersonAddressEndpointsTests : EndpointsTestBase
{
    public GetPersonAddressEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task GetPersonAddress_ShouldReturnNotFound_WhenRandomAddressIdIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.GetAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetPersonAddress_ShouldReturnNotFound_WhenRandomPersonIdIsProvided()
    {
        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.GetAsync(
            new Uri($"api/v1/persons/{Guid.NewGuid()}/addresses/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetPersonAddress_ShouldMapAllProperties_WhenAddressExists()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreatePersonAddressRequest request = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);
        AddressId addressId = await PersonAddressApiFactory.CreateAndGetIdAsync(ApiClient, personId, request);

        //Act
        PersonAddressResponse response = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, addressId);

        //Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(addressId);
        response.PersonId.ShouldBe(personId);
        response.Name.ShouldBe(request.Name);
        response.PostalCode.ShouldBe(request.PostalCode);
        response.Region.ShouldBe(request.Region);
        response.City.ShouldBe(request.City);
        response.Line.ShouldBe(request.Line);
        response.CountryCode.ShouldBe(request.TwoLetterIsoCountryCode);
    }

    [Fact]
    public async Task GetPersonAddress_ShouldReturnCorrectAddress_WhenMultipleAddressesExist()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreatePersonAddressRequest firstRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);
        CreatePersonAddressRequest secondRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);

        Task<AddressId> firstAddressIdTask = PersonAddressApiFactory.CreateAndGetIdAsync(ApiClient, personId, firstRequest);
        Task<AddressId> secondAddressIdTask = PersonAddressApiFactory.CreateAndGetIdAsync(ApiClient, personId, secondRequest);
        await Task.WhenAll(firstAddressIdTask, secondAddressIdTask);
        
        //Act
        PersonAddressResponse firstResponse = 
            await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, await firstAddressIdTask);
        PersonAddressResponse secondResponse = 
            await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, await secondAddressIdTask);

        //Assert
        firstResponse.Id.ShouldBe(await firstAddressIdTask);
        firstResponse.Name.ShouldBe(firstRequest.Name);

        secondResponse.Id.ShouldBe(await secondAddressIdTask);
        secondResponse.Name.ShouldBe(secondRequest.Name);
    }

    [Fact]
    public async Task GetPersonAddress_ShouldReturnNotFound_WhenAddressBelongsToDifferentPerson()
    {
        //Arrange
        Task<PersonId> firstPersonIdTask = PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        Task<PersonId> secondPersonIdTask = PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        await Task.WhenAll(firstPersonIdTask, secondPersonIdTask);

        AddressId addressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, await firstPersonIdTask);

        //Act
        PersonId secondPersonId = await secondPersonIdTask;
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.GetAsync(
            new Uri($"api/v1/persons/{secondPersonId.Value}/addresses/{addressId.Value}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}

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

public sealed class UpdatePersonAddressEndpointsTests : EndpointsTestBase
{
    public UpdatePersonAddressEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task UpdatePersonAddress_ShouldMapEveryRequestProperty_WhenValidDataIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        AddressId addressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        UpdatePersonAddressRequest request = PersonAddressApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PutAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{addressId.Value}", UriKind.Relative), request);

        //Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        PersonAddressResponse addressResponse = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, addressId);
        addressResponse.ShouldNotBeNull();
        addressResponse.Name.ShouldBe(request.Name);
        addressResponse.PostalCode.ShouldBe(request.PostalCode);
        addressResponse.Region.ShouldBe(request.Region);
        addressResponse.City.ShouldBe(request.City);
        addressResponse.Line.ShouldBe(request.Line);
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public async Task UpdatePersonAddress_ShouldReturnBadRequest_WhenAtLeastOneRequiredPropertyIsMissing(
        bool replaceNameWithNull,
        bool replacePostalCodeWithNull,
        bool replaceRegionWithNull,
        bool replaceCityWithNull)
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        AddressId addressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        PolishAddressData addressData = Faker.PolishAddress();

        UpdatePersonAddressRequest request = new(
            Name: replaceNameWithNull ? null! : Faker.Lorem.Word(),
            PostalCode: replacePostalCodeWithNull ? null! : addressData.PostalCode,
            Region: replaceRegionWithNull ? null! : addressData.Region,
            City: replaceCityWithNull ? null! : addressData.City,
            Line: Faker.Address.StreetAddress());

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PutAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{addressId.Value}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task UpdatePersonAddress_ShouldReturnNotFound_WhenNonExistingPersonIdIsProvided()
    {
        //Arrange
        UpdatePersonAddressRequest request = PersonAddressApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PutAsJsonAsync(
            new Uri($"api/v1/persons/{Guid.NewGuid()}/addresses/{Guid.NewGuid()}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePersonAddress_ShouldReturnNotFound_WhenNonExistingAddressIdIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        UpdatePersonAddressRequest request = PersonAddressApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PutAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{Guid.NewGuid()}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePersonAddress_ShouldReturnNotFound_WhenAddressBelongsToDifferentPerson()
    {
        //Arrange
        PersonId firstPersonId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        PersonId secondPersonId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        AddressId addressId = await PersonAddressApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, firstPersonId);
        UpdatePersonAddressRequest request = PersonAddressApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PutAsJsonAsync(
            new Uri($"api/v1/persons/{secondPersonId.Value}/addresses/{addressId.Value}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdatePersonAddress_ShouldNotAffectOtherAddresses_WhenUpdatingOne()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreatePersonAddressRequest firstCreateRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);
        CreatePersonAddressRequest secondCreateRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);

        AddressId firstAddressId = await PersonAddressApiFactory.CreateAndGetIdAsync(ApiClient, personId, firstCreateRequest);
        AddressId secondAddressId = await PersonAddressApiFactory.CreateAndGetIdAsync(ApiClient, personId, secondCreateRequest);

        UpdatePersonAddressRequest updateRequest = PersonAddressApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PutAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses/{firstAddressId.Value}", UriKind.Relative), updateRequest);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        PersonAddressResponse firstAddressResponse = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, firstAddressId);
        PersonAddressResponse secondAddressResponse = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, secondAddressId);

        firstAddressResponse.Name.ShouldBe(updateRequest.Name);
        secondAddressResponse.Name.ShouldBe(secondCreateRequest.Name);
    }
}

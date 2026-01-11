using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.Persons;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons.Addresses;

public sealed class CreatePersonAddressEndpointsTests : EndpointsTestBase
{
    public CreatePersonAddressEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task CreatePersonAddress_ShouldReturnAddressId_WhenValidDataIsProvided()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreatePersonAddressRequest request = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses", UriKind.Relative), request);

        //Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        AddressId addressId = JsonSerializer.Deserialize<AddressId>(stringResponse, ApiClient.JsonOptions);
        addressId.Value.ShouldNotBe(Guid.Empty);

        PersonAddressResponse addressResponse = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, personId, addressId);
        addressResponse.ShouldNotBeNull();
        addressResponse.PersonId.ShouldBe(personId);
        addressResponse.Name.ShouldBe(request.Name);
        addressResponse.PostalCode.ShouldBe(request.PostalCode);
        addressResponse.Region.ShouldBe(request.Region);
        addressResponse.City.ShouldBe(request.City);
        addressResponse.Line.ShouldBe(request.Line);
        addressResponse.CountryCode.ShouldBe(request.TwoLetterIsoCountryCode);
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public async Task CreatePersonAddress_ShouldReturnBadRequest_WhenAtLeastOneRequiredPropertyIsMissing(
        bool replaceNameWithNull,
        bool replacePostalCodeWithNull,
        bool replaceRegionWithNull,
        bool replaceCityWithNull)
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        PolishAddressData addressData = Faker.PolishAddress();

        CreatePersonAddressRequest request = new(
            TwoLetterIsoCountryCode: CountryCode.PL,
            Name: replaceNameWithNull ? null! : Faker.Lorem.Word(),
            PostalCode: replacePostalCodeWithNull ? null! : addressData.PostalCode,
            Region: replaceRegionWithNull ? null! : addressData.Region,
            City: replaceCityWithNull ? null! : addressData.City,
            Line: Faker.Address.StreetAddress());

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task CreatePersonAddress_ShouldReturnNotFound_WhenNonExistingPersonIdIsProvided()
    {
        //Arrange
        CreatePersonAddressRequest request = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
            new Uri($"api/v1/persons/{Guid.NewGuid()}/addresses", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task CreatePersonAddress_ShouldAllowCreatingMultipleAddresses_ForSamePerson()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CreatePersonAddressRequest firstRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);
        CreatePersonAddressRequest secondRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker);

        //Act
        Task<HttpResponseMessage> firstHttpResponseMessageTask = ApiClient.Http.PostAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses", UriKind.Relative), firstRequest);
        Task<HttpResponseMessage> secondHttpResponseMessageTask = ApiClient.Http.PostAsJsonAsync(
            new Uri($"api/v1/persons/{personId.Value}/addresses", UriKind.Relative), secondRequest);
        await Task.WhenAll(firstHttpResponseMessageTask, secondHttpResponseMessageTask);

        //Assert
        HttpResponseMessage firstHttpResponseMessage = await firstHttpResponseMessageTask;
        HttpResponseMessage secondHttpResponseMessage = await secondHttpResponseMessageTask;

        firstHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
        secondHttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        string firstStringResponse = await firstHttpResponseMessage.EnsureSuccessWithDetailsAsync();
        string secondStringResponse = await secondHttpResponseMessage.EnsureSuccessWithDetailsAsync();

        AddressId firstAddressId = JsonSerializer.Deserialize<AddressId>(firstStringResponse, ApiClient.JsonOptions);
        AddressId secondAddressId = JsonSerializer.Deserialize<AddressId>(secondStringResponse, ApiClient.JsonOptions);

        firstAddressId.Value.ShouldNotBe(secondAddressId.Value);

        PersonDetailsResponse personResponse = await PersonApiQueryService.GetByIdAsync(ApiClient, personId);
        personResponse.Addresses.Count.ShouldBe(2);
    }
}

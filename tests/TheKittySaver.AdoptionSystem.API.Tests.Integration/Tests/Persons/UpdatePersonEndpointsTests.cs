using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

public sealed class UpdatePersonEndpointsTests(TheKittySaverApiFactory appFactory)
    : PersonEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task UpdatePerson_ShouldMapEveryRequestProperty_WhenValidDataIsProvided()
    {
        //Arrange
        PersonDetailsResponse person = await PersonApiFactory.CreateRandomAsync(ApiClient, Faker);
        UpdatePersonRequest request = PersonApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/persons/{person.Id}", UriKind.Relative), request);

        //Assert
        _ = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        PersonDetailsResponse personResponse = await PersonApiQueryService.GetByIdAsync(ApiClient, person.Id);
        personResponse.ShouldNotBeNull();
        personResponse.Username.ShouldBe(request.Username);
        personResponse.Email.ShouldBe(request.Email);
        personResponse.PhoneNumber.ShouldBe(request.PhoneNumber);
        personResponse.Addresses.ShouldNotBeNull();
        personResponse.Addresses.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public async Task UpdatePerson_ShouldReturnBadRequest_WhenAtLeastOneRequiredPropertyIsMissing(
        bool replaceUsernameWithNull,
        bool replaceEmailWithNull,
        bool replacePhoneNumberWithNull)
    {
        //Arrange
        PersonDetailsResponse person = await PersonApiFactory.CreateRandomAsync(ApiClient, Faker);

        UpdatePersonRequest request = new(
            replaceUsernameWithNull ? null! : Faker.Internet.UserName(),
            replaceEmailWithNull ? null! : Faker.Internet.Email(),
            replacePhoneNumberWithNull ? null! : Faker.Person.PolishPhoneNumber());

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/persons/{person.Id}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task UpdatePerson_ShouldReturnNotFound_WhenNotExistingPersonIdIsProvided()
    {
        //Arrange
        UpdatePersonRequest request = PersonApiFactory.GenerateRandomUpdateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PutAsJsonAsync(
                new Uri($"api/v1/persons/{Guid.NewGuid()}", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}

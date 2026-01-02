using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

public sealed class CreatePersonEndpointsTests(TheKittySaverApiFactory appFactory)
    : PersonEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task CreatePerson_ShouldReturnPersonId_WhenValidDataIsProvided()
    {
        //Arrange
        CreatePersonRequest request = PersonApiFactory.GenerateRandomCreateRequest(Faker);

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PostAsJsonAsync(new Uri("api/v1/persons", UriKind.Relative), request);

        //Assert
        string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);

        PersonId personId = JsonSerializer.Deserialize<PersonId>(stringResponse, ApiClient.JsonOptions);
        personId.Value.ShouldNotBe(Guid.Empty);

        PersonDetailsResponse personResponse = await PersonApiQueryService.GetByIdAsync(ApiClient, personId);
        personResponse.ShouldNotBeNull();
        personResponse.Username.ShouldBe(request.Username);
        personResponse.Email.ShouldBe(request.Email);
        personResponse.PhoneNumber.ShouldBe(request.PhoneNumber);
        personResponse.Addresses.ShouldNotBeNull();
        personResponse.Addresses.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    public async Task CreatePerson_ShouldReturnBadRequest_WhenAtLeastOneRequiredPropertyIsMissing(
        bool replaceIdentityIdWithEmpty,
        bool replaceUsernameWithNull,
        bool replaceEmailWithNull,
        bool replacePhoneNumberWithNull)
    {
        //Arrange
        CreatePersonRequest request = new(
            replaceIdentityIdWithEmpty ? IdentityId.Empty : IdentityId.New(),
            replaceUsernameWithNull ? null! : Faker.Internet.UserName(),
            replaceEmailWithNull ? null! : Faker.Internet.Email(),
            replacePhoneNumberWithNull ? null! : Faker.Person.PolishPhoneNumber());

        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.PostAsJsonAsync(new Uri("api/v1/persons", UriKind.Relative), request);

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status400BadRequest);
    }
}

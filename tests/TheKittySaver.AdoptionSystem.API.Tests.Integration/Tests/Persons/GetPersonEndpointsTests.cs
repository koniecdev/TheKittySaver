using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

internal sealed class GetPersonEndpointsTests(TheKittySaverApiFactory appFactory)
    : PersonEndpointsTestBase(appFactory)
{
    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenRandomIdIsProvided()
    {
        //Act
        HttpResponseMessage httpResponseMessage =
            await ApiClient.Http.GetAsync(new Uri($"/api/v1/persons/{Guid.NewGuid()}", UriKind.Relative));

        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        ProblemDetails? problemDetails =
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(ApiClient.JsonOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetPerson_ShouldMapAllOfPersonProperties_WhenPersonsExists()
    {
        //Arrange
        CreatePersonRequest personRequest = PersonApiFactory.GenerateRandomCreateRequest(Faker);
        HttpResponseMessage createPersonHttpResponseMessage =
            await ApiClient.Http.PostAsJsonAsync(new Uri("/api/v1/persons", UriKind.Relative), personRequest);
        string stringResponse = await createPersonHttpResponseMessage.EnsureSuccessWithDetailsAsync();

        PersonId personId = JsonSerializer.Deserialize<PersonId>(stringResponse, ApiClient.JsonOptions);

        //Act
        PersonDetailsResponse response = await PersonApiQueryService.GetByIdAsync(ApiClient, personId);

        //Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(personId);
        response.Username.ShouldBe(personRequest.Username);
        response.Email.ShouldBe(personRequest.Email);
        response.PhoneNumber.ShouldBe(personRequest.PhoneNumber);
        response.Addresses.ShouldNotBeNull();
        response.CreatedAt.ShouldNotBe(new DateTimeOffset());
        response.CreatedAt.ShouldBeGreaterThan(new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));
        response.Addresses.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetPerson_ShouldReturnProperPersons_WhenMultiplePersonsExists()
    {
        //Arrange
        CreatePersonRequest personRequest = PersonApiFactory.GenerateRandomCreateRequest(Faker);
        HttpResponseMessage createPersonHttpResponseMessage =
            await ApiClient.Http.PostAsJsonAsync(new Uri("/api/v1/persons", UriKind.Relative), personRequest);
        string stringResponse = await createPersonHttpResponseMessage.EnsureSuccessWithDetailsAsync();
        PersonId personId = JsonSerializer.Deserialize<PersonId>(stringResponse, ApiClient.JsonOptions);

        CreatePersonRequest anotherPersonRequest = PersonApiFactory.GenerateRandomCreateRequest(Faker);
        _ = await ApiClient.Http.PostAsJsonAsync(new Uri("/api/v1/persons", UriKind.Relative), anotherPersonRequest);

        //Act
        PersonDetailsResponse response = await PersonApiQueryService.GetByIdAsync(ApiClient, personId);

        //Assert
        response.ShouldNotBeNull();
        response.Id.ShouldBe(personId);
    }
}

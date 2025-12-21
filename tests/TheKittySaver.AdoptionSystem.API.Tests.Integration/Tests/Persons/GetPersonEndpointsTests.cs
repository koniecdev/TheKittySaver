using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

[Collection("Api")]
public sealed class GetPersonEndpointsTests : AsyncLifetimeTestBase
{
    protected override HttpClient HttpClient { get; }
    protected override JsonSerializerOptions JsonSerializerOptions { get; }
    

    public GetPersonEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        HttpClient = appFactory.CreateClient();
        JsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptionsSnapshot<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task GetPerson_ShouldReturnNotFound_WhenRandomIdIsProvided()
    {
        //Act
        HttpResponseMessage httpResponseMessage = 
            await HttpClient.GetAsync(new Uri($"/api/v1/persons/{Guid.NewGuid()}", UriKind.Relative));
        
        //Assert
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        
        ProblemDetails? problemDetails = 
            await httpResponseMessage.Content.ReadFromJsonAsync<ProblemDetails>(JsonSerializerOptions);
        problemDetails.ShouldNotBeNull();
        problemDetails.Status.ShouldBe(StatusCodes.Status404NotFound);
    }
    
    [Fact]
    public async Task GetPerson_ShouldMapAllOfPersonProperties_WhenPersonsExists()
    {
        //Arrange
        CreatePersonRequest personRequest = PersonApiFactory.GenerateRandomCreateRequest(Faker);
        HttpResponseMessage createPersonHttpResponseMessage = 
            await HttpClient.PostAsJsonAsync(new Uri("/api/v1/persons", UriKind.Relative), personRequest);
        string stringResponse = await createPersonHttpResponseMessage.EnsureSuccessWithDetailsAsync();

        PersonId personId = JsonSerializer.Deserialize<PersonId>(stringResponse, JsonSerializerOptions);
        
        //Act
        HttpResponseMessage httpResponseMessage = 
            await HttpClient.GetAsync(new Uri($"/api/v1/persons/{personId}", UriKind.Relative));
        
        //Assert
        string content = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        PersonDetailsResponse response =
            JsonSerializer.Deserialize<PersonDetailsResponse>(content, JsonSerializerOptions)
            ?? throw new JsonException("Could not deserialize the response");

        response.ShouldNotBeNull();

        response.Id.ShouldBe(personId);
        response.Username.ShouldBe(personRequest.Username);
        response.Email.ShouldBe(personRequest.Email);
        response.PhoneNumber.ShouldBe(personRequest.PhoneNumber);
        response.Addresses.ShouldNotBeNull();
        response.Addresses.Count.ShouldBe(0);
    }
    
    [Fact]
    public async Task GetPerson_ShouldReturnProperPersons_WhenMultiplePersonsExists()
    {
        //Arrange
        CreatePersonRequest personRequest = PersonApiFactory.GenerateRandomCreateRequest(Faker);
        HttpResponseMessage createPersonHttpResponseMessage = 
            await HttpClient.PostAsJsonAsync(
                new Uri("/api/v1/persons", UriKind.Relative), personRequest, JsonSerializerOptions);
        string stringResponse = await createPersonHttpResponseMessage.EnsureSuccessWithDetailsAsync();
        PersonId personId = JsonSerializer.Deserialize<PersonId>(stringResponse, JsonSerializerOptions);
        
        CreatePersonRequest anotherPersonRequest = PersonApiFactory.GenerateRandomCreateRequest(Faker);
        _ = await HttpClient.PostAsJsonAsync(
            new Uri("/api/v1/persons", UriKind.Relative), anotherPersonRequest, JsonSerializerOptions);
        
        //Act
        HttpResponseMessage httpResponseMessage = 
            await HttpClient.GetAsync(new Uri($"/api/v1/persons/{personId}", UriKind.Relative));
        
        //Assert
        string content = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        PersonDetailsResponse response =
            JsonSerializer.Deserialize<PersonDetailsResponse>(content, JsonSerializerOptions)
            ?? throw new JsonException("Could not deserialize the response");

        response.ShouldNotBeNull();
        response.Id.ShouldBe(personId);
    }
}

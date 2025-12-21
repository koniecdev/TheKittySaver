using System.Net;
using System.Text.Json;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Extensions;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

[Collection("Api")]
public sealed class GetPersonsEndpointsTests : AsyncLifetimeTestBase
{
    protected override HttpClient HttpClient { get; }
    protected override JsonSerializerOptions JsonSerializerOptions { get; }

    public GetPersonsEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        HttpClient = appFactory.CreateClient();
        JsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptionsSnapshot<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task GetPersons_ShouldReturnEmptyItemList_WhenNoPersonsExists()
    {
        //Act
        HttpResponseMessage httpResponseMessage = 
            await HttpClient.GetAsync(new Uri("/api/v1/persons", UriKind.Relative));
        
        //Assert
        string content = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        PaginationResponse<PersonListItemResponse> response =
            JsonSerializer.Deserialize<PaginationResponse<PersonListItemResponse>>(content, JsonSerializerOptions)
            ?? throw new JsonException("Could not deserialize the response");

        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(0);
    }
    
    [Fact]
    public async Task GetPersons_ShouldMapAllOfPersonProperties_WhenPersonsExists()
    {
        //Arrange
        PersonDetailsResponse personResponse = 
            await PersonApiFactory.CreateRandomAsync(HttpClient, JsonSerializerOptions, Faker);
        
        //Act
        HttpResponseMessage httpResponseMessage = 
            await HttpClient.GetAsync(new Uri("/api/v1/persons", UriKind.Relative));
        
        //Assert
        string content = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        PaginationResponse<PersonListItemResponse> response =
            JsonSerializer.Deserialize<PaginationResponse<PersonListItemResponse>>(content, JsonSerializerOptions)
            ?? throw new JsonException("Could not deserialize the response");

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
        //Arrange
        _ = await PersonApiFactory.CreateRandomAsync(HttpClient, JsonSerializerOptions, Faker);
        _ = await PersonApiFactory.CreateRandomAsync(HttpClient, JsonSerializerOptions, Faker);
        
        //Act
        HttpResponseMessage httpResponseMessage = 
            await HttpClient.GetAsync(new Uri("/api/v1/persons", UriKind.Relative));
        
        //Assert
        string content = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
        httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.OK);
        
        PaginationResponse<PersonListItemResponse> response =
            JsonSerializer.Deserialize<PaginationResponse<PersonListItemResponse>>(content, JsonSerializerOptions)
            ?? throw new JsonException("Could not deserialize the response");

        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(2);
    }
}

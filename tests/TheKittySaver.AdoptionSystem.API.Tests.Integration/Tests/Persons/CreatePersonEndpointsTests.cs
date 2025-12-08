using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

[Collection("Api")]
public class CreatePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public CreatePersonEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions = 
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnUser_WhenValidDataIsProvided()
    {
        //Arrange
        
        CreatePersonRequest request = new(
            IdentityId.New(),
            "koniecdev",
            "koniecdev@gmail.com",
            "+48535143330");
        
        //Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request); 
        
        //Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();
        
        PersonResponse personResponse = JsonSerializer.Deserialize<PersonResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");
        personResponse.ShouldNotBeNull();
        personResponse.Username.ShouldBe("koniecdev");
        personResponse.Email.ShouldBe("koniecdev@gmail.com");
        personResponse.PhoneNumber.ShouldBe("+48535143330");
    }
    
    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}

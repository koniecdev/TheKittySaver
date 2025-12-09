using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shouldly;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Responses;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons;

[Collection("Api")]
public sealed class CreatePersonEndpointsTests : IAsyncLifetime
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly Faker _faker = new();

    public CreatePersonEndpointsTests(TheKittySaverApiFactory appFactory)
    {
        _httpClient = appFactory.CreateClient();
        _jsonSerializerOptions =
            appFactory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }

    [Fact]
    public async Task CreatePerson_ShouldReturnUser_WhenValidDataIsProvided()
    {
        // Arrange
        string username = _faker.Internet.UserName();
        string email = _faker.Internet.Email();

        CreatePersonRequest request = new(
            IdentityId.New(),
            username,
            email,
            $"+48{new Random().Next(100000000, 999999999)}");

        // Act
        HttpResponseMessage httpResponseMessage = await _httpClient.PostAsJsonAsync("api/v1/persons", request);

        // Assert
        httpResponseMessage.EnsureSuccessStatusCode();
        string stringResponse = await httpResponseMessage.Content.ReadAsStringAsync();

        PersonResponse personResponse = JsonSerializer.Deserialize<PersonResponse>(stringResponse, _jsonSerializerOptions)
            ?? throw new JsonException("Failed to deserialize PersonResponse");
        personResponse.ShouldNotBeNull();
        personResponse.Username.ShouldBe(username);
        personResponse.Email.ShouldBe(email);
        personResponse.PhoneNumber.ShouldBe($"+48{new Random().Next(100000000, 999999999)}");
    }

    public Task InitializeAsync() => Task.CompletedTask;
    public Task DisposeAsync() => Task.CompletedTask;
}

// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
// using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
// using TheKittySaver.AdoptionSystem.Contracts.Aggregates.PersonAggregate.Requests;
// using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
//
// namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.Persons.Addresses;
//
// public sealed class CreatePersonAddressEndpointsTests : EndpointsTestBase
// {
//     public CreatePersonAddressEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
//     {
//     }
//
//     [Fact]
//     public async Task CreatePersonAddress_ShouldBeSuccessful_WhenValidDataIsProvided()
//     {
//         //Arrange
//         PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
//         CreatePersonAddressRequest createPersonAddressRequest = PersonAddressApiFactory.GenerateRandomCreateRequest(Faker, personId);
//         
//         //Act
//         HttpResponseMessage httpResponseMessage = await ApiClient.Http.PostAsJsonAsync(
//             new Uri("api/v1/PersonAddresss", UriKind.Relative), createPersonAddressRequest);
//         
//         //Assert
//         httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
//         string stringResponse = await httpResponseMessage.EnsureSuccessWithDetailsAsync();
//         httpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.Created);
//
//         PersonAddressId PersonAddressId = JsonSerializer.Deserialize<PersonAddressId>(stringResponse, ApiClient.JsonOptions);
//         PersonAddressId.Value.ShouldNotBe(Guid.Empty);
//         
//         PersonAddressDetailsResponse PersonAddressDetailsResponse = await PersonAddressApiQueryService.GetByIdAsync(ApiClient, PersonAddressId);
//         PersonAddressDetailsResponse.ShouldNotBeNull();
//         PersonAddressDetailsResponse.PersonId.ShouldBe(personId);
//         PersonAddressDetailsResponse.Name.ShouldBe(createPersonAddressRequest.Name);
//         PersonAddressDetailsResponse.AdoptionAnnouncementId.ShouldBe(null);
//         PersonAddressDetailsResponse.AdoptionHistoryLastReturnDate.ShouldBe(createPersonAddressRequest.AdoptionHistoryLastReturnDate);
//     }
// }

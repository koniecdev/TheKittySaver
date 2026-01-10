using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Bases;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.AdoptionAnnouncements;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Cats;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.Factories.Persons;
using TheKittySaver.AdoptionSystem.API.Tests.Integration.Shared.QueryServices.AdoptionAnnouncements;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Requests;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.AdoptionAnnouncementAggregate.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;
using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.API.Tests.Integration.Tests.AdoptionAnnouncements;

public sealed class GetAdoptionAnnouncementsEndpointsTests : EndpointsTestBase
{
    public GetAdoptionAnnouncementsEndpointsTests(TheKittySaverApiFactory appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task GetAdoptionAnnouncements_ShouldReturnEmptyItemList_WhenNoAnnouncementsExist()
    {
        //Act
        PaginationResponse<AdoptionAnnouncementListItemResponse> response =
            await AdoptionAnnouncementApiQueryService.GetAllAsync(ApiClient);

        //Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(0);
    }

    [Fact]
    public async Task GetAdoptionAnnouncements_ShouldMapAllProperties_WhenAnnouncementExists()
    {
        //Arrange
        PersonId personId = await PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        CatId catId = await CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, personId);
        _ = await CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, catId);
        CreateAdoptionAnnouncementRequest request = 
            AdoptionAnnouncementApiFactory.GenerateRandomCreateRequest(Faker, [catId]);
        AdoptionAnnouncementId aaId = await AdoptionAnnouncementApiFactory.CreateAndGetIdAsync(ApiClient, request);

        //Act
        PaginationResponse<AdoptionAnnouncementListItemResponse> response =
            await AdoptionAnnouncementApiQueryService.GetAllAsync(ApiClient);

        //Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(1);

        AdoptionAnnouncementListItemResponse announcementFromList = response.Items.First();
        announcementFromList.Id.ShouldBe(aaId);
        announcementFromList.PersonId.ShouldBe(personId);
        announcementFromList.Title.ShouldNotBeEmpty();
        announcementFromList.Description.ShouldBe(request.Description);
        announcementFromList.AddressCountryCode.ShouldBe(request.AddressCountryCode);
        announcementFromList.AddressRegion.ShouldBe(request.AddressRegion);
        announcementFromList.AddressCity.ShouldBe(request.AddressCity);
        announcementFromList.AddressPostalCode.ShouldBe(request.AddressPostalCode);
        announcementFromList.AddressCity.ShouldBe(request.AddressCity);
        announcementFromList.AddressLine.ShouldBe(request.AddressLine);
        announcementFromList.Email.ShouldBe(request.Email);
        announcementFromList.PhoneNumber.ShouldBe(request.PhoneNumber);
    }

    [Fact]
    public async Task GetAdoptionAnnouncements_ShouldReturnMultipleAnnouncements_WhenMultipleAnnouncementsExist()
    {
        //Arrange
        Task<PersonId> personIdTask = PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        Task<PersonId> secondPersonIdTask = PersonApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker);
        await Task.WhenAll(personIdTask, secondPersonIdTask);
        
        Task<CatId> catIdTask = CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, await personIdTask);
        Task<CatId> secondCatIdTask = CatApiFactory.CreateRandomAndGetIdAsync(ApiClient, Faker, await secondPersonIdTask);
        await Task.WhenAll(catIdTask, secondCatIdTask);
        
        Task<CatThumbnailResponse> catThumbnailTask = 
            CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, await catIdTask);
        Task<CatThumbnailResponse> secondcatThumbnailTask = 
            CatGalleryApiFactory.UpsertRandomThumbnailAsync(ApiClient, await secondCatIdTask);
        await Task.WhenAll(catThumbnailTask, secondcatThumbnailTask);
        
        Task<AdoptionAnnouncementId> adoptionAnnouncementIdTask = AdoptionAnnouncementApiFactory
            .CreateRandomAndGetIdAsync(ApiClient, Faker, [await catIdTask]);
        Task<AdoptionAnnouncementId> secondAdoptionAnnouncementIdTask = AdoptionAnnouncementApiFactory
            .CreateRandomAndGetIdAsync(ApiClient, Faker, [await secondCatIdTask]);
        await Task.WhenAll(adoptionAnnouncementIdTask, secondAdoptionAnnouncementIdTask);
        
        //Act
        PaginationResponse<AdoptionAnnouncementListItemResponse> response =
            await AdoptionAnnouncementApiQueryService.GetAllAsync(ApiClient);

        //Assert
        response.ShouldNotBeNull();
        response.Items.ShouldNotBeNull();
        response.Items.Count.ShouldBe(2);
    }
}

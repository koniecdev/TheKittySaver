using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.AdoptionAnnouncementCreationServices;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Services;

public sealed class AdoptionAnnouncementCreationServiceTests
{
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset OperationDate = new(2025, 6, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly AdoptionAnnouncementCreationService _service;

    public AdoptionAnnouncementCreationServiceTests()
    {
        ICatAdoptionAnnouncementAssignmentService assignmentService =
            new CatAdoptionAnnouncementAssignmentService();
        _service = new AdoptionAnnouncementCreationService(assignmentService);
    }

    [Fact]
    public void Create_ShouldCreateAnnouncementAndAssignCat_WhenAllConditionsAreMet()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Value.PersonId.ShouldBe(cat.PersonId);

        // Verify cat was assigned atomically
        cat.Status.ShouldBe(CatStatusType.Published);
        cat.AdoptionAnnouncementId.ShouldBe(result.Value.Id);
        cat.PublishedAt.ShouldNotBeNull();
    }

    [Fact]
    public void Create_ShouldCreateAnnouncementWithDescription_WhenDescriptionIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        AdoptionAnnouncementDescription desc = AdoptionAnnouncementFactory.CreateRandomDescription(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.From(desc);

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Description.ShouldNotBeNull();
        result.Value.Description.ShouldBe(desc);
    }

    [Fact]
    public void Create_ShouldSetCorrectContactInformation_WhenCreatingAnnouncement()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Address.ShouldBe(address);
        result.Value.Email.ShouldBe(email);
        result.Value.PhoneNumber.ShouldBe(phoneNumber);
    }

    [Fact]
    public void Create_ShouldFail_WhenCatHasNoThumbnail()
    {
        //Arrange - Cat without thumbnail cannot be published
        Cat cat = CatFactory.CreateRandom(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        cat.Status.ShouldBe(CatStatusType.Draft); // Cat should remain Draft
    }

    [Fact]
    public void Create_ShouldFail_WhenCatIsNotInDraftStatus()
    {
        //Arrange - Cat already published
        PersonId personId = PersonId.Create();
        Cat cat = CatFactory.CreateWithThumbnail(Faker, personId: personId);
        Result assignToAdoptionAnnouncementResult = cat.AssignToAdoptionAnnouncement(
            AdoptionAnnouncementFactory.CreateRandom(Faker, personId: personId).Id,
            OperationDate.AddDays(-1));
        assignToAdoptionAnnouncementResult.EnsureSuccess();

        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
    }

    [Fact]
    public void Create_ShouldNotPublishCat_WhenAssignmentFails()
    {
        //Arrange - Create a cat that will fail assignment (no thumbnail)
        Cat cat = CatFactory.CreateRandom(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        CatStatusType originalStatus = cat.Status;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        cat.Status.ShouldBe(originalStatus); // Cat status should not change
        cat.AdoptionAnnouncementId.ShouldBeNull(); // Cat should not be assigned
    }

    [Fact]
    public void Create_ShouldEnsureAtomicity_WhenCreatingAnnouncementAndAssigningCat()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert - Both announcement creation AND cat assignment should succeed together
        result.IsSuccess.ShouldBeTrue();
        cat.Status.ShouldBe(CatStatusType.Published);
        cat.AdoptionAnnouncementId.ShouldNotBeNull();
        cat.AdoptionAnnouncementId.ShouldBe(result.Value.Id);
    }

    [Fact]
    public void Create_ShouldDelegateToAssignmentService_WhenAssigningCat()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert - Verify assignment service logic was applied (cat published with published date)
        result.IsSuccess.ShouldBeTrue();
        cat.PublishedAt.ShouldNotBeNull();
        cat.PublishedAt.Value.ShouldBe(OperationDate);
    }

    [Fact]
    public void Create_ShouldPassEmptyCatCollection_WhenCallingAssignmentService()
    {
        //Arrange - First cat in announcement, so no existing cats
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementAddress address = AdoptionAnnouncementFactory.CreateRandomAddress(Faker);
        Email email = AdoptionAnnouncementFactory.CreateRandomEmail(Faker);
        PhoneNumber phoneNumber = AdoptionAnnouncementFactory.CreateRandomPhoneNumber(Faker);
        Maybe<AdoptionAnnouncementDescription> description = Maybe<AdoptionAnnouncementDescription>.None;

        //Act
        Result<AdoptionAnnouncement> result = _service.Create(
            [cat],
            address,
            email,
            phoneNumber,
            description,
            OperationDate);

        //Assert - Should succeed as there are no existing cats to check compatibility with
        result.IsSuccess.ShouldBeTrue();
    }
}

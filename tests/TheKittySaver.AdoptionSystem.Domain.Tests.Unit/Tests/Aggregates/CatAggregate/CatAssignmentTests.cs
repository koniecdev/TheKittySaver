using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CatAssignmentTests
{
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset ValidOperationDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void AssignToAdoptionAnnouncement_ShouldAssign_WhenCatIsDraftWithThumbnail()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.AdoptionAnnouncementId.ShouldBe(announcementId);
        cat.Status.ShouldBe(CatStatusType.Published);
        cat.PublishedAt.ShouldNotBeNull();
    }

    [Fact]
    public void AssignToAdoptionAnnouncement_ShouldReturnFailure_WhenCatHasNoThumbnail()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.ThumbnailProperty.RequiredForPublishing(cat.Id));
    }

    [Fact]
    public void AssignToAdoptionAnnouncement_ShouldReturnFailure_WhenCatIsNotDraft()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId firstAnnouncementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(firstAnnouncementId, ValidOperationDate);

        AdoptionAnnouncementId secondAnnouncementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.AssignToAdoptionAnnouncement(secondAnnouncementId, ValidOperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.StatusProperty.MustBeDraftForAssignment(cat.Id));
    }

    [Fact]
    public void AssignToAdoptionAnnouncement_ShouldThrow_WhenEmptyAnnouncementIdIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);

        //Act
        Action assign = () => cat.AssignToAdoptionAnnouncement(AdoptionAnnouncementId.Empty, ValidOperationDate);

        //Assert
        assign.ShouldThrow<ArgumentException>()
            .ParamName?.ToLower().ShouldContain(nameof(Cat.AdoptionAnnouncementId));
    }

    [Fact]
    public void ReassignToAnotherAdoptionAnnouncement_ShouldReassign_WhenCatIsPublished()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId firstAnnouncementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(firstAnnouncementId, ValidOperationDate);

        AdoptionAnnouncementId secondAnnouncementId = AdoptionAnnouncementId.New();
        DateTimeOffset reassignDate = ValidOperationDate.AddDays(1);

        //Act
        Result result = cat.ReassignToAnotherAdoptionAnnouncement(secondAnnouncementId, reassignDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.AdoptionAnnouncementId.ShouldBe(secondAnnouncementId);
        cat.Status.ShouldBe(CatStatusType.Published);
    }

    [Fact]
    public void ReassignToAnotherAdoptionAnnouncement_ShouldReturnFailure_WhenCatIsNotPublished()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.ReassignToAnotherAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.StatusProperty.MustBePublishedForReassignment(cat.Id));
    }

    [Fact]
    public void ReassignToAnotherAdoptionAnnouncement_ShouldThrow_WhenEmptyAnnouncementIdIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId firstAnnouncementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(firstAnnouncementId, ValidOperationDate);

        //Act
        Action reassign = () => cat.ReassignToAnotherAdoptionAnnouncement(
            AdoptionAnnouncementId.Empty, ValidOperationDate);

        //Assert
        reassign.ShouldThrow<ArgumentException>()
            .ParamName?.ToLower().ShouldContain($"destination{nameof(Cat.AdoptionAnnouncementId)}");
    }

    [Fact]
    public void UnassignFromAdoptionAnnouncement_ShouldUnassign_WhenCatIsPublished()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Act
        Result result = cat.UnassignFromAdoptionAnnouncement();

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.AdoptionAnnouncementId.ShouldBeNull();
        cat.Status.ShouldBe(CatStatusType.Draft);
        cat.PublishedAt.ShouldBeNull();
    }

    [Fact]
    public void UnassignFromAdoptionAnnouncement_ShouldReturnFailure_WhenCatIsNotPublished()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Result result = cat.UnassignFromAdoptionAnnouncement();

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.StatusProperty.NotPublished(cat.Id));
    }

    #region Domain Events Tests

    [Fact]
    public void ReassignToAnotherAdoptionAnnouncement_ShouldRaiseCatReassignedDomainEvent_WhenSuccessful()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId firstAnnouncementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(firstAnnouncementId, ValidOperationDate);

        AdoptionAnnouncementId secondAnnouncementId = AdoptionAnnouncementId.New();
        DateTimeOffset reassignDate = ValidOperationDate.AddDays(1);

        //Act
        Result result = cat.ReassignToAnotherAdoptionAnnouncement(secondAnnouncementId, reassignDate);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
        events.ShouldContain(e => e is CatReassignedToAnotherAnnouncementDomainEvent);

        CatReassignedToAnotherAnnouncementDomainEvent reassignEvent =
            events.OfType<CatReassignedToAnotherAnnouncementDomainEvent>().First();
        reassignEvent.CatId.ShouldBe(cat.Id);
        reassignEvent.SourceAdoptionAnnouncementId.ShouldBe(firstAnnouncementId);
        reassignEvent.DestinationAdoptionAnnouncementId.ShouldBe(secondAnnouncementId);
    }

    [Fact]
    public void ReassignToAnotherAdoptionAnnouncement_ShouldNotRaiseDomainEvent_WhenReassignmentFails()
    {
        //Arrange - Draft cat cannot be reassigned
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();

        //Act
        Result result = cat.ReassignToAnotherAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Assert
        result.IsFailure.ShouldBeTrue();
        IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
        events.ShouldNotContain(e => e is CatReassignedToAnotherAnnouncementDomainEvent);
    }

    [Fact]
    public void UnassignFromAdoptionAnnouncement_ShouldRaiseCatUnassignedDomainEvent_WhenSuccessful()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Act
        Result result = cat.UnassignFromAdoptionAnnouncement();

        //Assert
        result.IsSuccess.ShouldBeTrue();
        IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
        events.ShouldContain(e => e is CatUnassignedFromAnnouncementDomainEvent);

        CatUnassignedFromAnnouncementDomainEvent unassignEvent =
            events.OfType<CatUnassignedFromAnnouncementDomainEvent>().First();
        unassignEvent.CatId.ShouldBe(cat.Id);
        unassignEvent.SourceAdoptionAnnouncementId.ShouldBe(announcementId);
    }

    [Fact]
    public void UnassignFromAdoptionAnnouncement_ShouldNotRaiseDomainEvent_WhenUnassignmentFails()
    {
        //Arrange - Draft cat cannot be unassigned
        Cat cat = CatFactory.CreateRandom(Faker);

        //Act
        Result result = cat.UnassignFromAdoptionAnnouncement();

        //Assert
        result.IsFailure.ShouldBeTrue();
        IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
        events.ShouldNotContain(e => e is CatUnassignedFromAnnouncementDomainEvent);
    }

    #endregion
}

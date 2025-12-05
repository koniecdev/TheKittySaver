using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.CatAggregate;

public sealed class CatClaimTests
{
    private static readonly Faker Faker = new();
    private static readonly DateTimeOffset ValidOperationDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Claim_ShouldClaimCat_WhenCatIsPublishedAndAssigned()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);
        ClaimedAt claimedAt = CatFactory.CreateDefaultClaimedAt();

        //Act
        Result result = cat.Claim(claimedAt);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        cat.Status.ShouldBe(CatStatusType.Claimed);
        cat.ClaimedAt.ShouldBe(claimedAt);
    }

    [Fact]
    public void Claim_ShouldReturnFailure_WhenCatIsDraft()
    {
        //Arrange
        Cat cat = CatFactory.CreateRandom(Faker);
        ClaimedAt claimedAt = CatFactory.CreateDefaultClaimedAt();

        //Act
        Result result = cat.Claim(claimedAt);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.StatusProperty.CannotClaimDraftCat(cat.Id));
    }

    [Fact]
    public void Claim_ShouldReturnFailure_WhenCatIsAlreadyClaimed()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);
        ClaimedAt claimedAt = CatFactory.CreateDefaultClaimedAt();
        cat.Claim(claimedAt);

        ClaimedAt secondClaimedAt = CatFactory.CreateDefaultClaimedAt();

        //Act
        Result result = cat.Claim(secondClaimedAt);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.CatEntity.StatusProperty.AlreadyClaimed(cat.Id));
    }

    [Fact]
    public void Claim_ShouldThrow_WhenNullClaimedAtIsProvided()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);

        //Act
        Action claim = () => cat.Claim(null!);

        //Assert
        claim.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldContain(nameof(Cat.ClaimedAt));
    }
    
    [Fact]
    public void Claim_ShouldRaiseCatClaimedDomainEvent_WhenSuccessful()
    {
        //Arrange
        Cat cat = CatFactory.CreateWithThumbnail(Faker);
        AdoptionAnnouncementId announcementId = AdoptionAnnouncementId.New();
        cat.AssignToAdoptionAnnouncement(announcementId, ValidOperationDate);
        ClaimedAt claimedAt = CatFactory.CreateDefaultClaimedAt();

        //Act
        Result result = cat.Claim(claimedAt);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
        events.ShouldNotBeEmpty();
        events.ShouldContain(e => e is CatClaimedDomainEvent);

        CatClaimedDomainEvent claimedEvent = events.OfType<CatClaimedDomainEvent>().First();
        claimedEvent.CatId.ShouldBe(cat.Id);
        claimedEvent.AdoptionAnnouncementId.ShouldBe(announcementId);
    }

    [Fact]
    public void Claim_ShouldNotRaiseDomainEvent_WhenClaimFails()
    {
        //Arrange - Draft cat cannot be claimed
        Cat cat = CatFactory.CreateRandom(Faker);
        ClaimedAt claimedAt = CatFactory.CreateDefaultClaimedAt();

        //Act
        Result result = cat.Claim(claimedAt);

        //Assert
        result.IsFailure.ShouldBeTrue();
        IReadOnlyCollection<IDomainEvent> events = cat.GetDomainEvents();
        events.ShouldNotContain(e => e is CatClaimedDomainEvent);
    }
}

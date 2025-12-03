using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate;

public sealed class AdoptionAnnouncementClaimTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void Claim_ShouldClaimAnnouncement_WhenAnnouncementIsActive()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        ClaimedAt claimedAt = AdoptionAnnouncementFactory.CreateDefaultClaimedAt();

        //Act
        Result result = announcement.Claim(claimedAt);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.Status.ShouldBe(AnnouncementStatusType.Claimed);
        announcement.ClaimedAt.ShouldBe(claimedAt);
    }

    [Fact]
    public void Claim_ShouldReturnFailure_WhenAnnouncementIsAlreadyClaimed()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        ClaimedAt claimedAt = AdoptionAnnouncementFactory.CreateDefaultClaimedAt();
        announcement.Claim(claimedAt);

        ClaimedAt secondClaimedAt = AdoptionAnnouncementFactory.CreateDefaultClaimedAt();

        //Act
        Result result = announcement.Claim(secondClaimedAt);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementErrors.StatusProperty.AlreadyClaimed(announcement.Id));
    }

    [Fact]
    public void Claim_ShouldThrow_WhenNullClaimedAtIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);

        //Act
        Action claim = () => announcement.Claim(null!);

        //Assert
        claim.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLower().ShouldContain("claimedat".ToLower());
    }

}

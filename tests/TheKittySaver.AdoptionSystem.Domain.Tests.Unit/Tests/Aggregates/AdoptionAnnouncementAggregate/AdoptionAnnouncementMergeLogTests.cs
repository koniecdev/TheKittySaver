using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate;

public sealed class AdoptionAnnouncementMergeLogTests
{
    private static readonly Faker Faker = new();

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldAddMergeLog_WhenValidIdIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementId deletedAnnouncementId = AdoptionAnnouncementId.Create();

        //Act
        Result result = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(deletedAnnouncementId);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.MergeLogs.Count.ShouldBe(1);
        announcement.MergeLogs[0].MergedAdoptionAnnouncementId.ShouldBe(deletedAnnouncementId);
    }

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldAddMultipleMergeLogs_WhenDifferentIdsAreProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementId firstDeletedAnnouncementId = AdoptionAnnouncementId.Create();
        AdoptionAnnouncementId secondDeletedAnnouncementId = AdoptionAnnouncementId.Create();

        //Act
        Result firstResult = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(firstDeletedAnnouncementId);
        Result secondResult = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(secondDeletedAnnouncementId);

        //Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        announcement.MergeLogs.Count.ShouldBe(2);
    }

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldReturnFailure_WhenDuplicateMergeLogIsAdded()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementId deletedAnnouncementId = AdoptionAnnouncementId.Create();
        announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(deletedAnnouncementId);

        //Act
        Result result = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(deletedAnnouncementId);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementErrors.MergeLogsProperty.AlreadyExists);
        announcement.MergeLogs.Count.ShouldBe(1);
    }

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldThrow_WhenEmptyIdIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);

        //Act
        Action addMergeLog = () => announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(AdoptionAnnouncementId.Empty);

        //Assert
        addMergeLog.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldContain("mergedadoptionannouncementid".ToLowerInvariant());
    }
}

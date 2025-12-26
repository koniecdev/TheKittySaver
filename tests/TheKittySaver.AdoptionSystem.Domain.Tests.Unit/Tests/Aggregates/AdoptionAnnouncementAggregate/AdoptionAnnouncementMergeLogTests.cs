using Bogus;
using Shouldly;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Tests.Aggregates.AdoptionAnnouncementAggregate;

public sealed class AdoptionAnnouncementMergeLogTests
{
    private static readonly Faker Faker = new();

    private static AdoptionAnnouncementMergetAt CreateValidMergedAt()
    {
        DateTimeOffset validDate = new(2025, 6, 1, 0, 0, 0, TimeSpan.Zero);
        Result<AdoptionAnnouncementMergetAt> result = AdoptionAnnouncementMergetAt.Create(validDate);
        return result.Value;
    }

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldAddMergeLog_WhenValidIdIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementId deletedAnnouncementId = AdoptionAnnouncementId.Create();
        AdoptionAnnouncementMergetAt mergedAt = CreateValidMergedAt();

        //Act
        Result result = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(
            deletedAnnouncementId,
            mergedAt);

        //Assert
        result.IsSuccess.ShouldBeTrue();
        announcement.MergeLogs.Count.ShouldBe(1);
        announcement.MergeLogs[0].MergedAdoptionAnnouncementId.ShouldBe(deletedAnnouncementId);
        announcement.MergeLogs[0].MergedAt.ShouldBe(mergedAt);
    }

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldAddMultipleMergeLogs_WhenDifferentIdsAreProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementId firstDeletedAnnouncementId = AdoptionAnnouncementId.Create();
        AdoptionAnnouncementId secondDeletedAnnouncementId = AdoptionAnnouncementId.Create();
        AdoptionAnnouncementMergetAt mergedAt = CreateValidMergedAt();

        //Act
        Result firstResult = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(
            firstDeletedAnnouncementId,
            mergedAt);
        Result secondResult = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(
            secondDeletedAnnouncementId,
            mergedAt);

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
        AdoptionAnnouncementMergetAt mergedAt = CreateValidMergedAt();
        announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(deletedAnnouncementId, mergedAt);

        //Act
        Result result = announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(
            deletedAnnouncementId,
            mergedAt);

        //Assert
        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(DomainErrors.AdoptionAnnouncementEntity.MergeLogsProperty.AlreadyExists);
        announcement.MergeLogs.Count.ShouldBe(1);
    }

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldThrow_WhenEmptyIdIsProvided()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementMergetAt mergedAt = CreateValidMergedAt();

        //Act
        Action addMergeLog = () => announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(
            AdoptionAnnouncementId.Empty,
            mergedAt);

        //Assert
        addMergeLog.ShouldThrow<ArgumentException>()
            .ParamName?.ToLowerInvariant().ShouldContain("deletedadoptionannouncementid".ToLowerInvariant());
    }

    [Fact]
    public void PersistAdoptionAnnouncementAfterLastCatReassignment_ShouldThrow_WhenMergedAtIsNull()
    {
        //Arrange
        AdoptionAnnouncement announcement = AdoptionAnnouncementFactory.CreateRandom(Faker);
        AdoptionAnnouncementId deletedAnnouncementId = AdoptionAnnouncementId.Create();

        //Act
        Action addMergeLog = () => announcement.PersistAdoptionAnnouncementAfterLastCatReassignment(
            deletedAnnouncementId,
            null!);

        //Assert
        addMergeLog.ShouldThrow<ArgumentNullException>()
            .ParamName?.ToLowerInvariant().ShouldContain("mergedat".ToLowerInvariant());
    }
}

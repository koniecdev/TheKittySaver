using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementMergeLog : ValueObject
{
    public AdoptionAnnouncementId MergedAdoptionAnnouncementId { get; }
    public AdoptionAnnouncementMergetAt MergedAt { get; }

    public static Result<AdoptionAnnouncementMergeLog> Create(
        AdoptionAnnouncementId mergedAdoptionAnnouncementId,
        AdoptionAnnouncementMergetAt mergedAt)
    {
        Ensure.NotEmpty(mergedAdoptionAnnouncementId);
        ArgumentNullException.ThrowIfNull(mergedAt);

        AdoptionAnnouncementMergeLog instance = new(mergedAdoptionAnnouncementId, mergedAt);
        return Result.Success(instance);
    }

    private AdoptionAnnouncementMergeLog(
        AdoptionAnnouncementId mergedAdoptionAnnouncementId,
        AdoptionAnnouncementMergetAt mergedAt)
    {
        MergedAdoptionAnnouncementId = mergedAdoptionAnnouncementId;
        MergedAt = mergedAt;
    }

    public override string ToString() => MergedAdoptionAnnouncementId.ToString();

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return MergedAdoptionAnnouncementId;
        yield return MergedAt;
    }
}

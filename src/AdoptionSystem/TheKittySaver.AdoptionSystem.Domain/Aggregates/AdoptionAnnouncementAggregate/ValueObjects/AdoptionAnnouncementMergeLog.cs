using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementMergeLog : ValueObject
{
    public AdoptionAnnouncementId MergedAdoptionAnnouncementId { get; }

    public static AdoptionAnnouncementMergeLog Create(AdoptionAnnouncementId mergedAdoptionAnnouncementId)
    {
        Ensure.NotEmpty(mergedAdoptionAnnouncementId);
        AdoptionAnnouncementMergeLog instance = new(mergedAdoptionAnnouncementId);
        return instance;
    }
    
    private AdoptionAnnouncementMergeLog(AdoptionAnnouncementId mergedAdoptionAnnouncementId)
    {
        MergedAdoptionAnnouncementId = mergedAdoptionAnnouncementId;
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return MergedAdoptionAnnouncementId;
    }
}
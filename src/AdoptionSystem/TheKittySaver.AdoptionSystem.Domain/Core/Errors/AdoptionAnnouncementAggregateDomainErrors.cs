using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class AdoptionAnnouncementEntity
    {
        public static Error NotFound(AdoptionAnnouncementId id)
            => HasNotBeenFound(
                nameof(AdoptionAnnouncementEntity),
                id.Value);
    }
}

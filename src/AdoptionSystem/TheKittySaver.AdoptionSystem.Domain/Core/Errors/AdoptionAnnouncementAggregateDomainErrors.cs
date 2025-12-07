using AdoptionAnnouncementEntity = TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities.AdoptionAnnouncement;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class AdoptionAnnouncementErrors
    {
        public static Error NotFound(AdoptionAnnouncementId id)
            => HasNotBeenFound(nameof(AdoptionAnnouncementErrors), id.Value);

        public static Error CannotDeleteAnnouncementWithClaimedCats
            => InvalidOperation(
                nameof(AdoptionAnnouncementErrors),
                "Cannot delete announcement with claimed cats. Please unassign cats that are not claimed first - this will make the announcement claimed as well.",
                nameof(CannotDeleteAnnouncementWithClaimedCats));
        
        public static class StatusProperty
        {
            public static Error UnavailableForAssigning
                => InvalidOperation(
                    nameof(AdoptionAnnouncementErrors),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Cat can be assigned to Announcement only when it is in Active status.",
                    nameof(UnavailableForAssigning));

            public static Error AlreadyClaimed(AdoptionAnnouncementId id)
                => StateConflict(
                    nameof(AdoptionAnnouncementErrors),
                    nameof(AdoptionAnnouncementEntity.Status),
                    $"Adoption announcement with id '{id.Value}' is already claimed.",
                    nameof(AlreadyClaimed));

            public static Error CanOnlyUpdateWhenActive
                => InvalidOperation(
                    nameof(AdoptionAnnouncementErrors),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Can only update announcement when it is active.",
                    nameof(CanOnlyUpdateWhenActive));

            public static Error CannotReassignCatToInactiveAnnouncement
                => InvalidOperation(
                    nameof(AdoptionAnnouncementErrors),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Cannot reassign cat to an adoption announcement that is not active.",
                    nameof(CannotReassignCatToInactiveAnnouncement));
            
        }

        public static class MergeLogsProperty
        {
            public static Error AlreadyExists
                => StateConflict(
                    nameof(AdoptionAnnouncementErrors),
                    nameof(AdoptionAnnouncementEntity.MergeLogs),
                    "Merge log for this adoption announcement already exists.",
                    nameof(AlreadyExists));
        }

        public static class DescriptionProperty
        {
            public static Error NullOrEmpty
                => Required(nameof(AdoptionAnnouncementErrors), nameof(AdoptionAnnouncementEntity.Description));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(AdoptionAnnouncementErrors),
                    nameof(AdoptionAnnouncementEntity.Description),
                    AdoptionAnnouncementDescription.MaxLength);
        }
    }
}

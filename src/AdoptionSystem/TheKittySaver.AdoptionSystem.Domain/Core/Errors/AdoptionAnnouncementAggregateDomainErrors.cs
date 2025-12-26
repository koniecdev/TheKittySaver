using AdoptionAnnouncementAggregate = TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities.AdoptionAnnouncement;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class AdoptionAnnouncementEntity
    {
        public static Error NotFound(AdoptionAnnouncementId id)
            => HasNotBeenFound(nameof(AdoptionAnnouncementEntity), id.Value);
        
        public static Error IsArchived(AdoptionAnnouncementId id)
            => InvalidOperation(
                nameof(AdoptionAnnouncementEntity),
                $"Adoption Announcement with ID '{id.Value}' is archived and cannot be modified.",
                "IsArchived");

        public static Error IsNotArchived(AdoptionAnnouncementId id)
            => InvalidOperation(
                nameof(AdoptionAnnouncementEntity),
                $"Adoption Announcement with ID '{id.Value}' is not archived.",
                "IsNotArchived");
        public static Error CannotDeleteAnnouncementWithClaimedCats
            => InvalidOperation(
                nameof(AdoptionAnnouncementEntity),
                "Cannot delete announcement with claimed cats. Please unassign cats that are not claimed first - this will make the announcement claimed as well.",
                nameof(CannotDeleteAnnouncementWithClaimedCats));
        
        public static class StatusProperty
        {
            public static Error UnavailableForAssigning
                => InvalidOperation(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.Status),
                    "Cat can be assigned to Announcement only when it is in Active status.",
                    nameof(UnavailableForAssigning));

            public static Error AlreadyClaimed(AdoptionAnnouncementId id)
                => StateConflict(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.Status),
                    $"Adoption announcement with id '{id.Value}' is already claimed.",
                    nameof(AlreadyClaimed));

            public static Error CanOnlyUpdateWhenActive
                => InvalidOperation(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.Status),
                    "Can only update announcement when it is active.",
                    nameof(CanOnlyUpdateWhenActive));

            public static Error CannotReassignCatFromInactiveAnnouncement
                => InvalidOperation(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.Status),
                    "Cannot reassign cat from an adoption announcement that is not active.",
                    nameof(CannotReassignCatFromInactiveAnnouncement));
            
            public static Error CannotReassignCatToInactiveAnnouncement
                => InvalidOperation(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.Status),
                    "Cannot reassign cat to an adoption announcement that is not active.",
                    nameof(CannotReassignCatToInactiveAnnouncement));
            
        }

        public static class MergeLogsProperty
        {
            public static Error AlreadyExists
                => StateConflict(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.MergeLogs),
                    "Merge log for this adoption announcement already exists.",
                    nameof(AlreadyExists));
        }

        public static class DescriptionProperty
        {
            public static Error NullOrEmpty
                => Required(nameof(AdoptionAnnouncementEntity), nameof(AdoptionAnnouncementAggregate.Description));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.Description),
                    AdoptionAnnouncementDescription.MaxLength);
            
            public static Error CanOnlyUpdateWhenActive
                => InvalidOperation(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncementAggregate.Description),
                    "Can only update announcement when it is active.",
                    nameof(CanOnlyUpdateWhenActive));
        }
    }
}

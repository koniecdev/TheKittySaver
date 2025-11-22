using AdoptionAnnouncementEntity = TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities.AdoptionAnnouncement;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class AdoptionAnnouncement
    {
        public static Error NotFound(AdoptionAnnouncementId id)
            => HasNotBeenFound(nameof(AdoptionAnnouncement), id.Value);

        public static class Status
        {
            public static Error UnavailableForAssigning
                => InvalidOperation(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Cat can be assigned to Announcement only when it is in Active status.",
                    "UnavailableForAssigning");

            public static Error AlreadyClaimed(AdoptionAnnouncementId id)
                => StateConflict(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Status),
                    $"Adoption announcement with id '{id.Value}' is already claimed.",
                    "AlreadyClaimed");

            public static Error CanOnlyUpdateWhenActive
                => InvalidOperation(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Can only update announcement when it is active.",
                    "CanOnlyUpdateWhenActive");

            public static Error CannotReassignCatToInactiveAnnouncement
                => InvalidOperation(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Cannot reassign cat to an adoption announcement that is not active.",
                    "CannotReassignCatToInactiveAnnouncement");
        }

        public static class MergeLogs
        {
            public static Error AlreadyExists
                => StateConflict(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.MergeLogs),
                    "Merge log for this adoption announcement already exists.",
                    "AlreadyExists");
        }

        public static class Description
        {
            public static Error NullOrEmpty
                => Required(nameof(AdoptionAnnouncement), nameof(AdoptionAnnouncementEntity.Description));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Description),
                    AdoptionAnnouncementDescription.MaxLength);
        }
    }
}

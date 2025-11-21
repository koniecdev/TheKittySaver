using AdoptionAnnouncementEntity = TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities.AdoptionAnnouncement;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
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
                    "Cat can be assigned to Announcement only when it is in Draft status.",
                    "UnavailableForAssigning");

            public static Error IsNotClaimed
                => InvalidOperation(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Announcement is not claimed.",
                    "IsNotClaimed");

            public static Error AlreadyClaimed(AdoptionAnnouncementId id)
                => StateConflict(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Status),
                    $"Adoption announcement with id '{id.Value}' is already claimed.",
                    "AlreadyClaimed");

            public static Error AlreadyArchived
                => StateConflict(
                    nameof(AdoptionAnnouncement),
                    nameof(AdoptionAnnouncementEntity.Status),
                    "Announcement is already archived.",
                    "AlreadyArchived");

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

        public static class CatsCompatibility
        {
            public static Error CannotMixInfectedWithHealthyCats
                => InvalidOperation(
                    nameof(AdoptionAnnouncement),
                    "CatsCompatibility",
                    "Cannot mix cats with FIV/FeLV positive status with FIV/FeLV negative cats in the same announcement.",
                    "CannotMixInfectedWithHealthyCats");
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

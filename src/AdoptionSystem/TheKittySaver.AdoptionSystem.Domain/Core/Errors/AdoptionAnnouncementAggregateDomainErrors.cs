using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
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

        public static Error UnavailableForAssigning
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Cat can be assigned to Announcement only when it is in Draft status.",
                "UnavailableForAssigning");
        
        public static Error IsNotClaimed
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Announcement is not claimed.",
                "IsNotClaimed");
        
        public static Error AlreadyClaimed
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Announcement is already claimed.");

        public static Error AdoptionAnnouncementAlreadyClaimed(AdoptionAnnouncementId id)
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                $"Adoption announcement with id '{id.Value}' is already claimed.");
        
        public static Error AlreadyArchived
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Announcement is already archived.");

        public static Error CanOnlyUpdateWhenActive
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Can only update announcement when it is active.");

        public static Error CannotMixInfectedWithHealthyCats
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                "CatsCompatibility",
                "Cannot mix cats with FIV/FeLV positive status with FIV/FeLV negative cats in the same announcement.");

        public static Error MergeLogAlreadyExists
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.MergeLogs),
                "Merge log for this adoption announcement already exists.",
                "AlreadyExists",
                TypeOfError.Conflict);

        public static Error CannotReassignCatToInactiveAdoptionAnnouncement
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Cannot reassign cat to an adoption announcement that is not active.");

        public static class DescriptionValueObject
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncement.Description));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncement.Description),
                    AdoptionAnnouncementDescription.MaxLength);
        }
    }
}
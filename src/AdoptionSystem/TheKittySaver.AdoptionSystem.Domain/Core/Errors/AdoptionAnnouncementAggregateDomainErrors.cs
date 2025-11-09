using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
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

        public static Error CanOnlyPublishDraft
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Only draft announcements can be published.");

        public static Error CanOnlyPauseActive
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Only active announcements can be paused.");

        public static Error CanOnlyResumeWhenPaused
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Only paused announcements can be resumed.");

        public static Error CanOnlyCompleteActiveOrPaused
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Only active or paused announcements can be completed.");

        public static Error CannotCancelFinishedAnnouncement
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Cannot cancel an announcement that is already completed or cancelled.");

        public static Error CannotMixInfectedWithHealthyCats
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                "CatsCompatibility",
                "Cannot mix cats with FIV/FeLV positive status with FIV/FeLV negative cats in the same announcement.");

        public static Error CanOnlyUpdateAddressWhenDraftActiveOrPaused
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Can only update address when announcement is in draft, active, or paused status.");

        public static Error CanOnlyUpdateEmailWhenDraftActiveOrPaused
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Can only update email when announcement is in draft, active, or paused status.");

        public static Error CanOnlyUpdatePhoneNumberWhenDraftActiveOrPaused
            => CustomMessage(
                nameof(AdoptionAnnouncementEntity),
                nameof(AdoptionAnnouncement.Status),
                "Can only update phone number when announcement is in draft, active, or paused status.");

        public static class DescriptionValueObject
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncement.Description));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(AdoptionAnnouncementEntity),
                    nameof(AdoptionAnnouncement.Description), AdoptionAnnouncementDescription.MaxLength);
        }

        public static class StatusValueObject
        {
            public static Error PauseReasonRequired
                => Required(nameof(AdoptionAnnouncementEntity), $"{nameof(AdoptionAnnouncement.Status)}.{nameof(AnnouncementStatus.StatusNote)}");

            public static Error CancelReasonRequired
                => Required(nameof(AdoptionAnnouncementEntity), $"{nameof(AdoptionAnnouncement.Status)}.{nameof(AnnouncementStatus.StatusNote)}");

            public static Error NoteTooLong
                => TooManyCharacters(nameof(AdoptionAnnouncementEntity), $"{nameof(AdoptionAnnouncement.Status)}.{nameof(AnnouncementStatus.StatusNote)}",
                    AnnouncementStatus.MaxStatusNoteLength);
        }
    }
}
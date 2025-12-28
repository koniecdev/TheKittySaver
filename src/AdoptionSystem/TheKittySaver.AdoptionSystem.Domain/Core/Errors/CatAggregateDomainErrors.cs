using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using AdoptionHistoryValueObject =
    TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects.AdoptionHistory;
using InfectiousDiseaseStatusValueObject =
    TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects.InfectiousDiseaseStatus;
using ListingSourceValueObject = TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects.ListingSource;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class CatEntity
    {
        public static Error NotFound(CatId id)
            => HasNotBeenFound(nameof(CatEntity), id.Value);

        public static Error IsArchived(CatId id)
            => InvalidOperation(
                nameof(CatEntity),
                $"Cat with ID '{id.Value}' is archived and cannot be modified.",
                "IsArchived");

        public static Error IsNotArchived(CatId id)
            => InvalidOperation(
                nameof(CatEntity),
                $"Cat with ID '{id.Value}' is not archived.",
                "IsNotArchived");

        public static Error CannotArchiveAssignedCat(CatId id)
            => InvalidOperation(
                nameof(CatEntity),
                $"Cat with ID '{id.Value}' cannot be archived because it is assigned to an adoption announcement. Unassign it first.",
                "CannotArchiveAssignedCat");

        public static Error GalleryIsFull
            => CustomMessage(
                nameof(CatEntity),
                "Gallery",
                $"Cannot add more items to the gallery. Maximum of {Cat.MaximumGalleryItemsCount} items allowed.",
                "GalleryIsFull",
                TypeOfError.Validation);

        public static Error InvalidReorderOperation
            => CustomMessage(
                nameof(CatEntity),
                "Gallery",
                "The reorder operation is invalid. The number of items in the new order must match the current gallery items count.",
                "InvalidReorderOperation",
                TypeOfError.Validation);

        public static Error TheOnlyAdoptionAnnouncementCatRemoval
            => InvalidDeleteOperation(
                nameof(CatEntity),
                "Cannot remove cat from an adoption announcement that has no other cats assigned to it. Please unassign it first.",
                "TheOnlyAdoptionAnnouncementCatRemoval");

        public static Error ClaimedCatRemoval
            => InvalidDeleteOperation(
                nameof(CatEntity),
                "Claimed cat cannot be deleted.",
                "ClaimedCatRemoval");

        public static Error DuplicateDisplayOrders
            => CustomMessage(
                nameof(CatEntity),
                "Gallery",
                "The reorder operation contains duplicate display order values.",
                "DuplicateDisplayOrders",
                TypeOfError.Validation);

        public static Error DisplayOrderMustBeContiguous
            => CustomMessage(
                nameof(CatEntity),
                "Gallery",
                "Display orders must be contiguous starting from 0.",
                "DisplayOrderMustBeContiguous",
                TypeOfError.Validation);

        public static class NameProperty
        {
            public static Error NullOrEmpty
                => Required(nameof(CatEntity), nameof(Cat.Name));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(CatEntity), nameof(Cat.Name), CatName.MaxLength);
        }

        public static class AgeProperty
        {
            public static Error BelowMinimalAllowedValue(int actualValue, int minimumValue)
                => BelowValue(nameof(CatEntity), nameof(Cat.Age), actualValue, minimumValue);

            public static Error AboveMaximumAllowedValue(int actualValue, int maximumValue)
                => AboveValue(nameof(CatEntity), nameof(Cat.Age), actualValue, maximumValue);
        }

        public static class AdoptionHistoryProperty
        {
            public static Error CountTooLow
                => CustomMessage(
                    nameof(CatEntity),
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistoryValueObject.ReturnCount)}",
                    "If cat has been returned, then it must have been returned at least one time.",
                    "CountTooLow",
                    TypeOfError.Validation);

            public static Error LastReturnTooFarInPast(DateTimeOffset lastReturnDate, DateTimeOffset currentDate)
                => CustomMessage(
                    nameof(CatEntity),
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistoryValueObject.LastReturnDate)}",
                    $"Last return date '{lastReturnDate:yyyy-MM-dd}' is too old to be valid (reference date: '{currentDate:yyyy-MM-dd}').",
                    "LastReturnTooFarInPast",
                    TypeOfError.Validation);

            public static Error LastReturnReasonIsEmpty
                => Required(nameof(CatEntity),
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistoryValueObject.LastReturnReason)}");

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(CatEntity),
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistoryValueObject.LastReturnReason)}",
                    AdoptionHistoryValueObject.LastReturnReasonMaxLength);
        }

        public static class ListingSourceProperty
        {
            public static Error SourceNameNullOrEmpty
                => Required(nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.{nameof(ListingSourceValueObject.SourceName)}");

            public static Error SourceNameLongerThanAllowed
                => TooManyCharacters(
                    nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.{nameof(ListingSourceValueObject.SourceName)}",
                    ListingSourceValueObject.MaxSourceNameLength);

            public static Error TypeIsUnset
                => Required(nameof(CatEntity), $"{nameof(Cat.ListingSource)}.{nameof(ListingSourceValueObject.Type)}");
        }

        public static class SpecialNeedsProperty
        {
            public static Error DescriptionNullOrEmpty
                => Required(nameof(CatEntity), $"{nameof(Cat.SpecialNeeds)}.{nameof(SpecialNeedsStatus.Description)}");

            public static Error DescriptionLongerThanAllowed
                => TooManyCharacters(
                    nameof(CatEntity),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(SpecialNeedsStatus.Description)}",
                    SpecialNeedsStatus.MaxDescriptionLength);

            public static Error SeverityIsUnset
                => Required(nameof(CatEntity), $"{nameof(Cat.SpecialNeeds)}.{nameof(SpecialNeedsStatus.SeverityType)}");
        }

        public static class DescriptionProperty
        {
            public static Error NullOrEmpty
                => Required(nameof(CatEntity), nameof(Cat.Description));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(CatEntity), nameof(Cat.Description), CatDescription.MaxLength);
        }

        public static class WeightProperty
        {
            public static Error BelowMinimum(decimal actual, decimal minimum)
                => BelowValue(nameof(CatEntity), nameof(Cat.Weight), actual, minimum);

            public static Error AboveMaximum(decimal actual, decimal maximum)
                => AboveValue(nameof(CatEntity), nameof(Cat.Weight), actual, maximum);
        }

        public static class InfectiousDiseaseStatusProperty
        {
            public static Error TestDateInFuture(DateOnly lastTestedAt, DateOnly currentDate)
                => CustomMessage(
                    nameof(CatEntity),
                    $"{nameof(Cat.InfectiousDiseaseStatus)}.{nameof(InfectiousDiseaseStatusValueObject.LastTestedAt)}",
                    $"Test date '{lastTestedAt:yyyy-MM-dd}' cannot be in the future (reference date: '{currentDate:yyyy-MM-dd}').",
                    "TestDateInFuture",
                    TypeOfError.Validation);

            public static Error TestDateTooOld(DateOnly lastTestedAt, DateOnly currentDate)
                => CustomMessage(
                    nameof(CatEntity),
                    $"{nameof(Cat.InfectiousDiseaseStatus)}.{nameof(InfectiousDiseaseStatusValueObject.LastTestedAt)}",
                    $"Test date '{lastTestedAt:yyyy-MM-dd}' is too old to be valid (reference date: '{currentDate:yyyy-MM-dd}').",
                    "TestDateTooOld",
                    TypeOfError.Validation);
        }

        public static class StatusProperty
        {
            public static Error AlreadyClaimed(CatId catId)
                => StateConflict(
                    nameof(CatEntity),
                    nameof(Cat.Status),
                    $"Cat with ID '{catId.Value}' has already been claimed.",
                    "AlreadyClaimed");

            public static Error MustBeDraftForAssignment(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.Status),
                    $"Cat with ID '{catId.Value}' must be in draft status to be assigned to an adoption announcement.",
                    "MustBeDraftForAssignment");

            public static Error CannotClaimDraftCat(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.Status),
                    $"Cannot claim cat with ID '{catId.Value}' because it is in draft status.",
                    "CannotClaimDraftCat");

            public static Error MustBePublishedForReassignment(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.Status),
                    $"Cat with ID '{catId.Value}' must be in published status to be reassigned to another adoption announcement.",
                    "MustBePublishedForReassignment");

            public static Error NotPublished(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.Status),
                    $"Cat with ID '{catId.Value}' is not published.",
                    "NotPublished");
        }

        public static class ThumbnailProperty
        {
            public static Error NotUploaded(CatId catId)
                => HasNotBeenFound(
                    $"{nameof(CatEntity)}.{nameof(Cat.Thumbnail)}",
                    catId.Value);

            public static Error RequiredForPublishing(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.Thumbnail),
                    $"Cat with ID '{catId.Value}' must have a thumbnail before being published.",
                    "RequiredForPublishing");

            public static Error InvalidStatusForUpsertThumbnailOperation(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.Thumbnail),
                    $"Cat with ID '{catId.Value}' must be in draft or published status to modify thumbnail.",
                    "InvalidStatusForThumbnailOperation");

            public static Error CannotRemoveFromPublishedCat(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.Thumbnail),
                    $"Cannot remove thumbnail from cat with ID '{catId.Value}' because it is published.",
                    "CannotRemoveFromPublishedCat");
        }

        public static class Assignment
        {
            public static Error AlreadyAssignedToAnotherAnnouncement(CatId catId)
                => StateConflict(
                    nameof(CatEntity),
                    nameof(Cat.AdoptionAnnouncementId),
                    $"Cat with ID '{catId.Value}' is already assigned to an adoption announcement.",
                    "AlreadyAssignedToAnotherAnnouncement");

            public static Error AlreadyAssignedToAnnouncement(CatId catId)
                => StateConflict(
                    nameof(CatEntity),
                    nameof(Cat.AdoptionAnnouncementId),
                    $"Cat with ID '{catId.Value}' is already assigned to this adoption announcement.",
                    "AlreadyAssignedToAnnouncement");

            public static Error NotAssignedToAdoptionAnnouncement(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.AdoptionAnnouncementId),
                    $"Cat with ID '{catId.Value}' is not assigned to any adoption announcement.",
                    "NotAssignedToAdoptionAnnouncement");

            public static Error CannotReassignToSameAnnouncement(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.AdoptionAnnouncementId),
                    $"Cat with ID '{catId.Value}' cannot be reassigned to the same adoption announcement it is already assigned to.",
                    "CannotReassignToSameAnnouncement");

            public static Error IncompatibleInfectiousDiseaseStatus(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.InfectiousDiseaseStatus),
                    $"Cat with ID '{catId.Value}' cannot be reassigned to an adoption announcement with incompatible infectious disease status.",
                    "IncompatibleInfectiousDiseaseStatus");

            public static Error CannotReassignToAnotherOwner(CatId catId)
                => InvalidOperation(
                    nameof(CatEntity),
                    nameof(Cat.PersonId),
                    $"Cat with ID '{catId.Value}' cannot be reassigned to an adoption announcement belonging to another owner.",
                    "CannotReassignToAnotherOwner");
        }
    }

    public static class CatGalleryItemEntity
    {
        public static Error NotFound(CatGalleryItemId id)
            => HasNotBeenFound(nameof(CatGalleryItemEntity), id.Value);

        public static class DisplayOrderProperty
        {
            public static Error BelowMinimum(int actualValue, int minimumValue)
                => BelowValue(nameof(CatGalleryItemEntity), nameof(CatGalleryItemDisplayOrder), actualValue,
                    minimumValue);

            public static Error AboveOrEqualMaximum(int actualValue, int maximumValue)
                => CustomMessage(
                    nameof(CatGalleryItemEntity),
                    nameof(CatGalleryItemDisplayOrder),
                    $"The displayorder has been set with value '{actualValue}', and it must be less than '{maximumValue}'.",
                    "AboveOrEqualMaximum",
                    TypeOfError.Validation);
        }
    }

    public static class VaccinationEntity
    {
        public static Error NotFound(VaccinationId vaccinationId)
            => HasNotBeenFound(nameof(VaccinationEntity), vaccinationId.Value);

        public static Error IsArchived(VaccinationId vaccinationId)
            => InvalidOperation(
                nameof(VaccinationEntity),
                $"Vaccination with ID '{vaccinationId.Value}' is archived and cannot be modified.",
                "IsArchived");

        public static Error IsNotArchived(VaccinationId vaccinationId)
            => InvalidOperation(
                nameof(VaccinationEntity),
                $"Vaccination with ID '{vaccinationId.Value}' is not archived.",
                "IsNotArchived");

        public static class VeterinarianNoteProperty
        {
            public static Error NullOrEmpty
                => Required(nameof(VaccinationEntity), nameof(Vaccination.VeterinarianNote));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(VaccinationEntity), nameof(Vaccination.VeterinarianNote),
                    VaccinationNote.MaxLength);
        }

        public static class DateProperty
        {
            public static Error VaccinationDateInFuture(DateOnly value, DateOnly referenceDate)
                => CustomMessage(
                    nameof(VaccinationEntity),
                    $"{nameof(VaccinationDate)}.{nameof(VaccinationDate.Value)}",
                    $"Vaccination date '{value:yyyy-MM-dd}' cannot be in the future (reference date: '{referenceDate:yyyy-MM-dd}').",
                    "VaccinationDateInFuture",
                    TypeOfError.Validation);

            public static Error VaccinationDateTooOld(DateOnly value, DateOnly referenceDate)
                => CustomMessage(
                    nameof(VaccinationEntity),
                    $"{nameof(VaccinationDate)}.{nameof(VaccinationDate.Value)}",
                    $"Vaccination date '{value:yyyy-MM-dd}' is too old to be valid (reference date: '{referenceDate:yyyy-MM-dd}').",
                    "VaccinationDateTooOld",
                    TypeOfError.Validation);
        }
    }
}

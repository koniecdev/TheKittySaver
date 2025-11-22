using CatEntity = TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities.Cat;
using VaccinationEntity = TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities.Vaccination;
using AdoptionHistoryValueObject = TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects.AdoptionHistory;
using ListingSourceValueObject = TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects.ListingSource;
using InfectiousDiseaseStatusValueObject = TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects.InfectiousDiseaseStatus;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class Cat
    {
        public static Error NotFound(CatId id)
            => HasNotBeenFound(nameof(Cat), id.Value);

        public static class Name
        {
            public static Error NullOrEmpty
                => Required(nameof(Cat), nameof(CatEntity.Name));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(Cat), nameof(CatEntity.Name), CatName.MaxLength);
        }

        public static class Age
        {
            public static Error BelowMinimalAllowedValue(int actualValue, int minimumValue)
                => BelowValue(nameof(Cat), nameof(CatEntity.Age), actualValue, minimumValue);

            public static Error AboveMaximumAllowedValue(int actualValue, int maximumValue)
                => AboveValue(nameof(Cat), nameof(CatEntity.Age), actualValue, maximumValue);
        }

        public static class AdoptionHistory
        {
            public static Error CountTooLow
                => CustomMessage(
                    nameof(Cat),
                    $"{nameof(CatEntity.AdoptionHistory)}.{nameof(AdoptionHistoryValueObject.ReturnCount)}",
                    "If cat has been returned, then it must have been returned at least one time.",
                    "CountTooLow",
                    TypeOfError.Validation);

            public static Error LastReturnTooFarInPast(DateTimeOffset lastReturnDate, DateTimeOffset currentDate)
                => CustomMessage(
                    nameof(Cat),
                    $"{nameof(CatEntity.AdoptionHistory)}.{nameof(AdoptionHistoryValueObject.LastReturnDate)}",
                    $"Last return date '{lastReturnDate:yyyy-MM-dd}' is too old to be valid (reference date: '{currentDate:yyyy-MM-dd}').",
                    "LastReturnTooFarInPast",
                    TypeOfError.Validation);

            public static Error LastReturnReasonIsEmpty
                => Required(nameof(Cat), $"{nameof(CatEntity.AdoptionHistory)}.{nameof(AdoptionHistoryValueObject.LastReturnReason)}");
        }

        public static class ListingSource
        {
            public static Error SourceNameNullOrEmpty
                => Required(nameof(Cat), $"{nameof(CatEntity.ListingSource)}.{nameof(ListingSourceValueObject.SourceName)}");

            public static Error SourceNameLongerThanAllowed
                => TooManyCharacters(
                    nameof(Cat),
                    $"{nameof(CatEntity.ListingSource)}.{nameof(ListingSourceValueObject.SourceName)}",
                    ListingSourceValueObject.MaxSourceNameLength);

            public static Error TypeIsUnset
                => Required(nameof(Cat), $"{nameof(CatEntity.ListingSource)}.{nameof(ListingSourceValueObject.Type)}");
        }

        public static class SpecialNeeds
        {
            public static Error DescriptionNullOrEmpty
                => Required(nameof(Cat), $"{nameof(CatEntity.SpecialNeeds)}.{nameof(SpecialNeedsStatus.Description)}");

            public static Error DescriptionLongerThanAllowed
                => TooManyCharacters(
                    nameof(Cat),
                    $"{nameof(CatEntity.SpecialNeeds)}.{nameof(SpecialNeedsStatus.Description)}",
                    SpecialNeedsStatus.MaxDescriptionLength);

            public static Error SeverityIsUnset
                => Required(nameof(Cat), $"{nameof(CatEntity.SpecialNeeds)}.{nameof(SpecialNeedsStatus.SeverityType)}");
        }

        public static class Description
        {
            public static Error NullOrEmpty
                => Required(nameof(Cat), nameof(CatEntity.Description));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(Cat), nameof(CatEntity.Description), CatDescription.MaxLength);
        }

        public static class Weight
        {
            public static Error BelowMinimum(decimal actual, decimal minimum)
                => BelowValue(nameof(Cat), nameof(CatEntity.Weight), actual, minimum);

            public static Error AboveMaximum(decimal actual, decimal maximum)
                => AboveValue(nameof(Cat), nameof(CatEntity.Weight), actual, maximum);
        }

        public static class InfectiousDiseaseStatus
        {
            public static Error TestDateInFuture(DateOnly lastTestedAt, DateOnly currentDate)
                => CustomMessage(
                    nameof(Cat),
                    $"{nameof(CatEntity.InfectiousDiseaseStatus)}.{nameof(InfectiousDiseaseStatusValueObject.LastTestedAt)}",
                    $"Test date '{lastTestedAt:yyyy-MM-dd}' cannot be in the future (reference date: '{currentDate:yyyy-MM-dd}').",
                    "TestDateInFuture",
                    TypeOfError.Validation);

            public static Error TestDateTooOld(DateOnly lastTestedAt, DateOnly currentDate)
                => CustomMessage(
                    nameof(Cat),
                    $"{nameof(CatEntity.InfectiousDiseaseStatus)}.{nameof(InfectiousDiseaseStatusValueObject.LastTestedAt)}",
                    $"Test date '{lastTestedAt:yyyy-MM-dd}' is too old to be valid (reference date: '{currentDate:yyyy-MM-dd}').",
                    "TestDateTooOld",
                    TypeOfError.Validation);
        }

        public static class Status
        {
            public static Error AlreadyClaimed(CatId catId)
                => StateConflict(
                    nameof(Cat),
                    nameof(CatEntity.Status),
                    $"Cat with ID '{catId.Value}' has already been claimed.",
                    "AlreadyClaimed");

            public static Error MustBeDraftForAssignment(CatId catId)
                => InvalidOperation(
                    nameof(Cat),
                    nameof(CatEntity.Status),
                    $"Cat with ID '{catId.Value}' must be in draft status to be assigned to an adoption announcement.",
                    "MustBeDraftForAssignment");

            public static Error CannotClaimDraftCat(CatId catId)
                => InvalidOperation(
                    nameof(Cat),
                    nameof(CatEntity.Status),
                    $"Cannot claim cat with ID '{catId.Value}' because it is in draft status.",
                    "CannotClaimDraftCat");

            public static Error MustBePublishedForReassignment(CatId catId)
                => InvalidOperation(
                    nameof(Cat),
                    nameof(CatEntity.Status),
                    $"Cat with ID '{catId.Value}' must be in published status to be reassigned to another adoption announcement.",
                    "MustBePublishedForReassignment");

            public static Error NotPublished(CatId catId)
                => InvalidOperation(
                    nameof(Cat),
                    nameof(CatEntity.Status),
                    $"Cat with ID '{catId.Value}' is not published.",
                    "NotPublished");
        }

        public static class Assignment
        {
            public static Error AlreadyAssignedToAnnouncement(CatId catId)
                => StateConflict(
                    nameof(Cat),
                    nameof(CatEntity.AdoptionAnnouncementId),
                    $"Cat with ID '{catId.Value}' is already assigned to this adoption announcement.",
                    "AlreadyAssignedToAnnouncement");

            public static Error NotAssignedToAdoptionAnnouncement(CatId catId)
                => InvalidOperation(
                    nameof(Cat),
                    nameof(CatEntity.AdoptionAnnouncementId),
                    $"Cat with ID '{catId.Value}' is not assigned to any adoption announcement.",
                    "NotAssignedToAdoptionAnnouncement");

            public static Error CannotReassignToSameAnnouncement(CatId catId)
                => InvalidOperation(
                    nameof(Cat),
                    nameof(CatEntity.AdoptionAnnouncementId),
                    $"Cat with ID '{catId.Value}' cannot be reassigned to the same adoption announcement it is already assigned to.",
                    "CannotReassignToSameAnnouncement");

            public static Error IncompatibleInfectiousDiseaseStatus(CatId catId)
                => InvalidOperation(
                    nameof(Cat),
                    nameof(CatEntity.InfectiousDiseaseStatus),
                    $"Cat with ID '{catId.Value}' cannot be reassigned to an adoption announcement with incompatible infectious disease status.",
                    "IncompatibleInfectiousDiseaseStatus");
        }
    }

    public static class CatGalleryItem
    {
        public static Error NotFound(CatGalleryItemId id)
            => HasNotBeenFound(nameof(CatGalleryItem), id.Value);

        public static class DisplayOrder
        {
            public static Error BelowMinimum(int actualValue, int minimumValue)
                => BelowValue(nameof(CatGalleryItem), nameof(CatGalleryItemDisplayOrder), actualValue, minimumValue);

            public static Error AboveOrEqualMaximum(int actualValue, int maximumValue)
                => CustomMessage(
                    nameof(CatGalleryItem),
                    nameof(CatGalleryItemDisplayOrder),
                    $"The displayorder has been set with value '{actualValue}', and it must be less than '{maximumValue}'.",
                    "AboveOrEqualMaximum",
                    TypeOfError.Validation);
        }
    }

    public static class Vaccination
    {
        public static Error NotFound(VaccinationId vaccinationId)
            => HasNotBeenFound(nameof(Vaccination), vaccinationId.Value);

        public static class VeterinarianNote
        {
            public static Error NullOrEmpty
                => Required(nameof(Vaccination), nameof(VaccinationEntity.VeterinarianNote));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(Vaccination), nameof(VaccinationEntity.VeterinarianNote), VaccinationNote.MaxLength);
        }

        public static class Dates
        {
            public static Error VaccinationDateInFuture(DateTimeOffset vaccinationDate, DateTimeOffset referenceDate)
                => CustomMessage(
                    nameof(Vaccination),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.VaccinationDate)}",
                    $"Vaccination date '{vaccinationDate:yyyy-MM-dd}' cannot be in the future (reference date: '{referenceDate:yyyy-MM-dd}').",
                    "VaccinationDateInFuture",
                    TypeOfError.Validation);

            public static Error VaccinationDateTooOld(DateTimeOffset vaccinationDate, DateTimeOffset referenceDate)
                => CustomMessage(
                    nameof(Vaccination),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.VaccinationDate)}",
                    $"Vaccination date '{vaccinationDate:yyyy-MM-dd}' is too old to be valid (reference date: '{referenceDate:yyyy-MM-dd}').",
                    "VaccinationDateTooOld",
                    TypeOfError.Validation);

            public static Error NextDueDateInPast(DateTimeOffset nextDueDate, DateTimeOffset referenceDate)
                => CustomMessage(
                    nameof(Vaccination),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.NextDueDate)}",
                    $"Next due date '{nextDueDate:yyyy-MM-dd}' cannot be in the past (reference date: '{referenceDate:yyyy-MM-dd}').",
                    "NextDueDateInPast",
                    TypeOfError.Validation);

            public static Error NextDueDateBeforeOrEqualVaccinationDate(DateTimeOffset nextDueDate, DateTimeOffset vaccinationDate)
                => CustomMessage(
                    nameof(Vaccination),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.NextDueDate)}",
                    $"Next due date '{nextDueDate:yyyy-MM-dd}' must be after the vaccination date '{vaccinationDate:yyyy-MM-dd}'.",
                    "NextDueDateBeforeOrEqualVaccinationDate",
                    TypeOfError.Validation);
        }
    }
}

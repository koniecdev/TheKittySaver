using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class CatEntity
    {
        public static Error NotFound(CatId id)
            => HasNotBeenFound(
                nameof(CatEntity),
                id.Value);

        public static Error VaccinationNotFound(VaccinationId vaccinationId)
            => HasNotBeenFound(
                nameof(CatEntity),
                vaccinationId.Value);
        
        public static class NameValueObject
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(CatEntity),
                    nameof(Cat.Name));
            
            public static Error LongerThanAllowed 
                => TooManyCharacters(
                    nameof(CatEntity),
                    nameof(Cat.Name),
                    CatName.MaxLength);
        }
        
        public static class AgeValueObject
        {
            public static Error BelowMinimalAllowedValue(
                int actualValue,
                int minimumValue) 
                => BelowValue(
                    nameof(CatEntity),
                    nameof(Cat.Age),
                    actualValue,
                    minimumValue);
            
            public static Error AboveMaximumAllowedValue(
                int actualValue,
                int maximumValue)
                => AboveValue(
                    nameof(CatEntity),
                    nameof(Cat.Age),
                    actualValue,
                    maximumValue);
        }
        
        public static class AdoptionHistoryValueObject
        {
            public static Error CountTooLow
                => CustomMessage(
                    nameof(CatEntity), 
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistory.ReturnCount)}",
                    "If cat has been returned, then it must has been returned at least one time.");
            
            public static Error LastReturnTooFarInPast(
                DateTimeOffset lastReturnDate,
                DateTimeOffset currentDate)
                => CustomMessage(
                    nameof(CatEntity),
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistory.LastReturnDate)}",
                    $"Last return date '{lastReturnDate:yyyy-MM-dd}' is too old to be valid (reference date: '{currentDate:yyyy-MM-dd}').");
            
            public static Error LastReturnReasonIsEmpty
                => Required(
                    nameof(CatEntity), 
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistory.LastReturnReason)}");
        }
        
        public static class ListingSourceValueObject
        {
            public static Error SourceNameIsNullOrEmpty 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.{nameof(ListingSource.SourceName)}");
            
            public static Error SourceNameIsLongerThanAllowed 
                => TooManyCharacters(
                    nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.{nameof(ListingSource.SourceName)}",
                    ListingSource.MaxSourceNameLength);
            
            public static Error TypeIsUnset 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.{nameof(ListingSource.Type)}");
        }
        
        public static class SpecialNeedsValueObject
        {
            public static Error DescriptionIsNullOrEmpty 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(SpecialNeedsStatus.Description)}");
            
            public static Error DescriptionIsLongerThanAllowed 
                => TooManyCharacters(
                    nameof(CatEntity),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(SpecialNeedsStatus.Description)}",
                    SpecialNeedsStatus.MaxDescriptionLength);
            
            public static Error SeverityIsUnset 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(SpecialNeedsStatus.SeverityType)}");
        }
        
        public static class DescriptionValueObject
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(CatEntity),
                    nameof(Cat.Description));
            
            public static Error LongerThanAllowed 
                => TooManyCharacters(
                    nameof(CatEntity),
                    nameof(Cat.Description),
                    CatDescription.MaxLength);
        }

        public static class WeightValueObject
        {
            public static Error BelowMinimum(decimal actual, decimal minimum)
                => BelowValue(
                    nameof(CatEntity), 
                    nameof(Cat.Weight), actual, minimum);
            
            public static Error AboveMaximum(decimal actual, decimal maximum)
                => AboveValue(
                    nameof(CatEntity),
                    nameof(Cat.Weight), actual, maximum);
        }


        public static class StatusValueObject
        {
            public static Error UnavailableReasonRequired
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.Status)}.{nameof(CatStatus.StatusNote)}");

            public static Error NoteTooManyCharacters
                => TooManyCharacters(
                    nameof(CatEntity),
                    $"{nameof(Cat.Status)}.{nameof(CatStatus.StatusNote)}",
                    CatStatus.MaxStatusNoteLength);
        }

        public static class InfectiousDiseaseStatusValueObject
        {
            public static Error TestDateInFuture(
                DateOnly lastTestedAt,
                DateOnly currentDate)
                => CustomMessage(
                    nameof(CatEntity),
                    $"{nameof(Cat.InfectiousDiseaseStatus)}.{nameof(InfectiousDiseaseStatus.LastTestedAt)}",
                    $"Test date '{lastTestedAt:yyyy-MM-dd}' cannot be in the future (reference date: '{currentDate:yyyy-MM-dd}').");

            public static Error TestDateTooOld(
                DateOnly lastTestedAt,
                DateOnly currentDate)
                => CustomMessage(
                    nameof(CatEntity),
                    $"{nameof(Cat.InfectiousDiseaseStatus)}.{nameof(InfectiousDiseaseStatus.LastTestedAt)}",
                    $"Test date '{lastTestedAt:yyyy-MM-dd}' is too old to be valid (reference date: '{currentDate:yyyy-MM-dd}').");
        }
    }

    public static class CatVaccinationEntity
    {
        public static Error NotFound(VaccinationId vaccinationId)
            => HasNotBeenFound(
                nameof(CatVaccinationEntity),
                vaccinationId.Value);
        
        public static class VeterinarianNoteValueObject
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(CatVaccinationEntity),
                    nameof(Vaccination.VeterinarianNote));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(CatVaccinationEntity),
                    nameof(Vaccination.VeterinarianNote),
                    VaccinationNote.MaxLength);
        }

        public static class VaccinationDatesValueObject
        {
            public static Error VaccinationDateInFuture(
                DateTimeOffset vaccinationDate, 
                DateTimeOffset referenceDate)
                => CustomMessage(
                    nameof(CatVaccinationEntity),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.VaccinationDate)}",
                    $"Vaccination date '{vaccinationDate:yyyy-MM-dd}' cannot be in the future (reference date: '{referenceDate:yyyy-MM-dd}').");

            public static Error VaccinationDateTooOld(
                DateTimeOffset vaccinationDate,
                DateTimeOffset referenceDate)
                => CustomMessage(
                    nameof(CatVaccinationEntity),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.VaccinationDate)}",
                    $"Vaccination date '{vaccinationDate:yyyy-MM-dd}' is too old to be valid (reference date: '{referenceDate:yyyy-MM-dd}').");

            public static Error NextDueDateInPast(
                DateTimeOffset nextDueDate,
                DateTimeOffset referenceDate)
                => CustomMessage(
                    nameof(CatVaccinationEntity),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.NextDueDate)}",
                    $"Next due date '{nextDueDate:yyyy-MM-dd}' cannot be in the past (reference date: '{referenceDate:yyyy-MM-dd}').");

            public static Error NextDueDateBeforeOrEqualVaccinationDate(
                DateTimeOffset nextDueDate,
                DateTimeOffset vaccinationDate)
                => CustomMessage(
                    nameof(CatVaccinationEntity),
                    $"{nameof(VaccinationDates)}.{nameof(VaccinationDates.NextDueDate)}",
                    $"Next due date '{nextDueDate:yyyy-MM-dd}' must be after the vaccination date '{vaccinationDate:yyyy-MM-dd}'.");
        }
    }
}
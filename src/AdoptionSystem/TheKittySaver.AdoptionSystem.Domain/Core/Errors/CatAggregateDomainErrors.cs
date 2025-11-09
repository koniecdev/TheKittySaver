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
            public static Error BelowMinimalAllowedValue(int actualValue, int minimumValue) 
                => BelowValue(
                    nameof(CatEntity),
                    nameof(Cat.Age),
                    actualValue,
                    minimumValue);
            
            public static Error AboveMaximumAllowedValue(int actualValue, int maximumValue)
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
            
            public static Error LastReturnTooFarInPast
                => CustomMessage(
                    nameof(CatEntity), 
                    $"{nameof(Cat.AdoptionHistory)}.{nameof(AdoptionHistory.LastReturnDate)}",
                    "Invalid last return date has been provided.");
            
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
                => BelowValue(nameof(CatEntity), nameof(Cat.Weight), actual, minimum);
            
            public static Error AboveMaximum(decimal actual, decimal maximum)
                => AboveValue(nameof(CatEntity), nameof(Cat.Weight), actual, maximum);
        }

        public static class NeuteringStatusValueObject
        {
            public static Error DateInFuture
                => CustomMessage(nameof(CatEntity), $"{nameof(Cat.NeuteringStatus)}.Date", 
                    "Neutering date cannot be in the future.");
            
            public static Error DateTooOld
                => CustomMessage(nameof(CatEntity), $"{nameof(Cat.NeuteringStatus)}.Date",
                    "Neutering date is too old to be valid.");
        }

        public static class StatusValueObject
        {
            public static Error UnavailableReasonRequired
                => Required(nameof(CatEntity), $"{nameof(Cat.Status)}.Reason");
            
            public static Error NoteTooManyCharacters
                => TooManyCharacters(nameof(CatEntity), $"{nameof(Cat.Status)}.Note",
                    CatStatus.MaxStatusNoteLength);
        }

        public static class InfectiousDiseaseStatusValueObject
        {
            public static Error TestDateInFuture
                => CustomMessage(nameof(CatEntity), $"{nameof(Cat.InfectiousDiseaseStatus)}.TestDate",
                    "Test date cannot be in the future.");

            public static Error TestDateTooOld
                => CustomMessage(nameof(CatEntity), $"{nameof(Cat.InfectiousDiseaseStatus)}.TestDate",
                    "Test date is too old to be valid.");
        }
    }

    public static class CatVaccinationEntity
    {
        public static Error NotFound(VaccinationId vaccinationId)
            => HasNotBeenFound(
                nameof(CatVaccinationEntity),
                vaccinationId.Value);

        public static Error DateInFuture
            => CustomMessage(nameof(CatVaccinationEntity), nameof(Vaccination.VaccinationDate),
                "Vaccination date cannot be in the future.");

        public static Error DateTooOld
            => CustomMessage(nameof(CatVaccinationEntity), nameof(Vaccination.VaccinationDate),
                "Vaccination date is too old to be valid.");

        public static Error NextDueDateInPast
            => CustomMessage(nameof(CatVaccinationEntity), nameof(Vaccination.NextDueDate),
                "Next due date cannot be in the past.");

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
    }
}
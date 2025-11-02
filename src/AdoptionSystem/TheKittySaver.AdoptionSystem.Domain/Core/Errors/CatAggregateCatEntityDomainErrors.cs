using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class CatEntity
    {
        public static class CatNameProperty
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(Cat),
                    nameof(Cat.Name));
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Cat),
                    nameof(Cat.Name),
                    CatName.MaxLength);
        }
        
        public static class CatAgeProperty
        {
            public static Error BelowMinimalAllowedValue(int actualValue, int minimumValue) 
                => BelowValue(
                    nameof(Cat),
                    nameof(Cat.Age),
                    actualValue,
                    minimumValue);
            public static Error AboveMaximumAllowedValue(int actualValue, int maximumValue)
                => AboveValue(
                    nameof(Cat),
                    nameof(Cat.Age),
                    actualValue,
                    maximumValue);
        }
        
        public static class AdoptionHistoryProperty
        {
            public static Error CountTooLow
                => CustomMessage(nameof(Cat), nameof(Cat.AdoptionHistory.ReturnCount),
                    "If cat has been returned, then it must has been returned at least one time.");
            public static Error LastReturnTooFarInPast
                => CustomMessage(nameof(Cat), nameof(Cat.AdoptionHistory.LastReturnDate),
                    "Invalid last return date has been provided.");
            public static Error LastReturnReasonIsEmpty
                => Required(nameof(Cat), nameof(Cat.AdoptionHistory.LastReturnReason));
        }
        
        public static class ListingSourceProperty
        {
            public static Error SourceNameIsNullOrEmpty 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.ListingSource)}.{nameof(Cat.ListingSource.SourceName)}");
            
            public static Error SourceNameIsLongerThanAllowed 
                => TooLong(
                    nameof(Cat),
                    $"{nameof(Cat.ListingSource)}.{nameof(Cat.ListingSource.SourceName)}",
                    ListingSource.MaxSourceNameLength);
            
            public static Error TypeIsUnset 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.ListingSource.Type)}");
        }
        
        public static class SpecialNeedsStatusProperty
        {
            public static Error DescriptionIsNullOrEmpty 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.SpecialNeeds.Description)}");
            
            public static Error DescriptionIsLongerThanAllowed 
                => TooLong(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.SpecialNeeds.Description)}",
                    SpecialNeedsStatus.MaxDescriptionLength);
            
            public static Error SpecialNeedsSeverityIsUnset 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.SpecialNeeds.SeverityType)}");
        }
    }
}
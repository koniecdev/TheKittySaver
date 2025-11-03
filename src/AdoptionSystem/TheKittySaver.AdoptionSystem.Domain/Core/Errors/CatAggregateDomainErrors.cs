using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
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
        
        public static class NameValueObject
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(CatEntity),
                    nameof(Cat.Name));
            
            public static Error LongerThanAllowed 
                => TooLong(
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
                    $"{nameof(Cat.AdoptionHistory)}.ReturnCount",
                    "If cat has been returned, then it must has been returned at least one time.");
            
            public static Error LastReturnTooFarInPast
                => CustomMessage(
                    nameof(CatEntity), 
                    $"{nameof(Cat.AdoptionHistory)}.LastReturnDate",
                    "Invalid last return date has been provided.");
            
            public static Error LastReturnReasonIsEmpty
                => Required(
                    nameof(CatEntity), 
                    $"{nameof(Cat.AdoptionHistory)}.LastReturnReason");
        }
        
        public static class ListingSourceValueObject
        {
            public static Error SourceNameIsNullOrEmpty 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.SourceName");
            
            public static Error SourceNameIsLongerThanAllowed 
                => TooLong(
                    nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.SourceName",
                    ListingSource.MaxSourceNameLength);
            
            public static Error TypeIsUnset 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.ListingSource)}.Type");
        }
        
        public static class SpecialNeedsValueObject
        {
            public static Error DescriptionIsNullOrEmpty 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.SpecialNeeds)}.Description");
            
            public static Error DescriptionIsLongerThanAllowed 
                => TooLong(
                    nameof(CatEntity),
                    $"{nameof(Cat.SpecialNeeds)}.Description",
                    SpecialNeedsStatus.MaxDescriptionLength);
            
            public static Error SeverityIsUnset 
                => Required(
                    nameof(CatEntity),
                    $"{nameof(Cat.SpecialNeeds)}.SeverityType");
        }
        
        public static class AdoptionPriorityScoreValueObject
        {
            public static Error BelowMinimalAllowedValue(decimal actualValue, decimal minimumValue) 
                => BelowValue(
                    nameof(Cat),
                    nameof(AdoptionPriorityScore),
                    actualValue,
                    minimumValue);
        
            public static Error AboveMaximumAllowedValue(decimal actualValue, decimal maximumValue)
                => AboveValue(
                    nameof(Cat),
                    nameof(AdoptionPriorityScore),
                    actualValue,
                    maximumValue);
        }
    }
}
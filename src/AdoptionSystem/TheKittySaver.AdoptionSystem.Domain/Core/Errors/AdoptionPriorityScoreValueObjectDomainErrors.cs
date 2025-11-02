using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
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

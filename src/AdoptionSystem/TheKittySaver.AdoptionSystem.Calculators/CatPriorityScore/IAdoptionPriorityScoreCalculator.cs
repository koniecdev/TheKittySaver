using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Calculators.CatPriorityScore;

public interface IAdoptionPriorityScoreCalculator
{
    public decimal Calculate(
        int returnCount,
        int age,
        ColorType color,
        CatGenderType gender,
        HealthStatusType healthStatus,
        ListingSourceType listingSource,
        SpecialNeedsSeverityType specialNeedsSeverity,
        TemperamentType temperament);
}
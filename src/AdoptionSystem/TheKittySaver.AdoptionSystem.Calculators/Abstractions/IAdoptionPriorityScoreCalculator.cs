using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Calculators.Abstractions;

public interface IAdoptionPriorityScoreCalculator
{
    public decimal Calculate(
        int returnCount,
        int age,
        ColorType color,
        CatGenderType gender,
        HealthStatusType healthStatus,
        SpecialNeedsSeverityType specialNeedsSeverityType,
        TemperamentType temperament,
        FivStatus fivStatus,
        FelvStatus felvStatus,
        bool isNeutered);
}

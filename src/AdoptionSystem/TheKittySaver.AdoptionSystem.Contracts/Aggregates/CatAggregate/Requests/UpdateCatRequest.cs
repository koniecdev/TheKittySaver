using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;

public sealed record UpdateCatRequest(
    string Name,
    string Description,
    int Age,
    CatGenderType Gender,
    ColorType Color,
    int WeightInGrams,
    HealthStatusType HealthStatus,
    bool HasSpecialNeeds,
    string? SpecialNeedsDescription,
    SpecialNeedsSeverityType SpecialNeedsSeverityType,
    TemperamentType Temperament,
    int AdoptionHistoryReturnCount,
    DateTimeOffset? AdoptionHistoryLastReturnDate,
    string? AdoptionHistoryLastReturnReason,
    bool IsNeutered,
    FivStatus FivStatus,
    FelvStatus FelvStatus,
    DateOnly? InfectiousDiseaseStatusLastTestedAt
);


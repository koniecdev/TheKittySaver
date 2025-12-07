using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Requests;

public sealed record CreateCatRequest(
    PersonId PersonId,
    string Name,
    string Description,
    int Age,
    CatGenderType Gender,
    ColorType Color,
    decimal WeightValueInKilograms,
    HealthStatusType HealthStatus,
    bool SpecialNeedsStatusHasSpecialNeeds,
    string? SpecialNeedsStatusDescription,
    SpecialNeedsSeverityType SpecialNeedsStatusSeverityType,
    TemperamentType Temperament,
    int AdoptionHistoryReturnCount,
    DateTimeOffset? AdoptionHistoryLastReturnDate,
    string? AdoptionHistoryLastReturnReason,
    ListingSourceType ListingSourceType,
    string ListingSourceSourceName,
    bool IsNeutered,
    FivStatus InfectiousDiseaseStatusFivStatus,
    FelvStatus InfectiousDiseaseStatusFelvStatus,
    DateOnly InfectiousDiseaseStatusLastTestedAt);
    

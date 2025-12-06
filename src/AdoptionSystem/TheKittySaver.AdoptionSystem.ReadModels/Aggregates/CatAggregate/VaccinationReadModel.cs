using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

public sealed record VaccinationReadModel(
    VaccinationId Id,
    CatId CatId,
    VaccinationType Type,
    DateOnly VaccinationDate,
    string? VeterinarianNote);

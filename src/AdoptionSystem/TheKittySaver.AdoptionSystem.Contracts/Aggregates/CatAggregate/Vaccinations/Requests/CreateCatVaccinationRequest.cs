using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Requests;

public sealed record CreateCatVaccinationRequest(
    VaccinationType Type,
    DateOnly VaccinationDate,
    string? VeterinarianNote = null
);

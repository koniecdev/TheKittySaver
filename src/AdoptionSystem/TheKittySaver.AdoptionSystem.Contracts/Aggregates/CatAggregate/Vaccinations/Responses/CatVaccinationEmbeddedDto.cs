using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;

public sealed record CatVaccinationEmbeddedDto(
    VaccinationId Id,
    VaccinationType Type,
    DateOnly VaccinationDate,
    string? VeterinarianNote);

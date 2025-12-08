using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Vaccinations.Responses;

public sealed record CatVaccinationResponse(
    VaccinationId Id,
    CatId CatId,
    VaccinationType Type,
    DateOnly VaccinationDate,
    string? VeterinarianNote) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}

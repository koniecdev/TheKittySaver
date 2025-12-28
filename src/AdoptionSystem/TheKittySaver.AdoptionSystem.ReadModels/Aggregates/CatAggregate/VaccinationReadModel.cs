using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.ReadModels.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

public sealed record VaccinationReadModel(
    VaccinationId Id,
    CatId CatId,
    VaccinationType Type,
    DateOnly VaccinationDate,
    string? VeterinarianNote,
    DateTimeOffset CreatedAt) : IReadOnlyEntity<VaccinationId>
{
    public CatReadModel Cat { get; init; } = null!;
}

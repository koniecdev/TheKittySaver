using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

public sealed record CatThumbnailReadModel(
    CatThumbnailId Id,
    CatId CatId,
    DateTimeOffset CreatedAt) : IReadOnlyEntity<CatThumbnailId>;

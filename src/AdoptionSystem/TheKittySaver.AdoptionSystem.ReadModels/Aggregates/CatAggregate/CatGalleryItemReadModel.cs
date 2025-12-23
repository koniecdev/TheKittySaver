using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.ReadModels.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

public sealed record CatGalleryItemReadModel(
    CatGalleryItemId Id,
    CatId CatId,
    int DisplayOrder,
    DateTimeOffset CreatedAt) : IReadOnlyEntity<CatGalleryItemId>;

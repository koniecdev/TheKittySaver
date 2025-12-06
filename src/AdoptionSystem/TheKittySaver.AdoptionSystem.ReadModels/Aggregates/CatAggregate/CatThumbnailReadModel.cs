using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

public sealed record CatThumbnailReadModel(
    CatThumbnailId Id,
    CatId CatId);

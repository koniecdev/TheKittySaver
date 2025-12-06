using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.ReadModels.Aggregates.CatAggregate;

public sealed record CatGalleryItemReadModel(
    CatGalleryItemId Id,
    CatId CatId,
    int DisplayOrder);

using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;

public sealed record CatGalleryItemEmbeddedDto(
    CatGalleryItemId Id,
    int DisplayOrder);

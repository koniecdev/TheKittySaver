namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;

public sealed record CatGalleryItemEmbeddedDto(
    string Path,
    int DisplayOrder);

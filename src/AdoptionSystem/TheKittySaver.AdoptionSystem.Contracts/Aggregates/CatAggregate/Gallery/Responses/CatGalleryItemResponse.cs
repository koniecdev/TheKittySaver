using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;

public sealed record CatGalleryItemResponse(
    CatGalleryItemId Id,
    CatId CatId,
    int DisplayOrder) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}

using TheKittySaver.AdoptionSystem.Contracts.Common;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Responses;

public sealed record CatThumbnailResponse(
    CatThumbnailId Id,
    CatId CatId) : ILinksResponse
{
    public IReadOnlyCollection<LinkDto> Links { get; set; } = [];
}

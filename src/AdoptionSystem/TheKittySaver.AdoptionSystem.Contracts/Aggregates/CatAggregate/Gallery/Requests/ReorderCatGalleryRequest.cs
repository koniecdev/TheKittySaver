using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Requests;

public sealed record ReorderCatGalleryRequest(
    IReadOnlyList<GalleryItemOrderEntry> NewOrders
);

public sealed record GalleryItemOrderEntry(
    CatGalleryItemId GalleryItemId,
    int DisplayOrder
);

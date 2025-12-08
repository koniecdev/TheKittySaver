namespace TheKittySaver.AdoptionSystem.Contracts.Aggregates.CatAggregate.Gallery.Requests;

public sealed record ReorderCatGalleryRequest(
    IReadOnlyList<GalleryItemOrderEntry> NewOrders
);

public sealed record GalleryItemOrderEntry(
    Guid GalleryItemId,
    int DisplayOrder
);

namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct CatGalleryItemId : IStronglyTypedId<CatGalleryItemId>
{
    public static CatGalleryItemId Create() => new(Guid.CreateVersion7());
    public static CatGalleryItemId Create(Guid id) => new(id);
}

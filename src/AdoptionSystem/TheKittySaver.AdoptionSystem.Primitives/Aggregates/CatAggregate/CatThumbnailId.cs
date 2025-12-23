namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct CatThumbnailId : IStronglyTypedId<CatThumbnailId>
{
    public static CatThumbnailId Create() => new(Guid.CreateVersion7());
    public static CatThumbnailId Create(Guid id) => new(id);
}

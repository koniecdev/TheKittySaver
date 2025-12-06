namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct CatId : IStronglyTypedId<CatId>
{
    public static CatId Create(Guid id) => new(id);
}

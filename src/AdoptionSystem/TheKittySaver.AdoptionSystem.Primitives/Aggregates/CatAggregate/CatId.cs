using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct CatId : IStronglyTypedId<CatId>
{
    public static CatId Create() => new(Guid.CreateVersion7());
    public static CatId Create(Guid id)
    {
        Ensure.NotEmpty(id);
        return new CatId(id);
    }
}

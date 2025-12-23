namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct IdentityId : IStronglyTypedId<IdentityId>
{
    public static IdentityId Create() => new(Guid.CreateVersion7());
    public static IdentityId Create(Guid id) => new(id);
}

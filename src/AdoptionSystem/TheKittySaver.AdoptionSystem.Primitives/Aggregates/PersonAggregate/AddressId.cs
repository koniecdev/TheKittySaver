namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct AddressId : IStronglyTypedId<AddressId>
{
    public static AddressId Create() => new(Guid.CreateVersion7());
    public static AddressId Create(Guid id) => new(id);
}

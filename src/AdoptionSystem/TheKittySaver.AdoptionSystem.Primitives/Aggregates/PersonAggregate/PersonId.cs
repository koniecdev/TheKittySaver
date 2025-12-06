namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct PersonId : IStronglyTypedId<PersonId>
{
    public static PersonId Create(Guid id) => new(id);
}

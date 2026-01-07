using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

[StronglyTypedId(jsonConverter: StronglyTypedIdJsonConverter.SystemTextJson)]
public partial struct PersonId : IStronglyTypedId<PersonId>
{
    public static PersonId Create() => new(Guid.CreateVersion7());
    public static PersonId Create(Guid id)
    {
        Ensure.NotEmpty(id);
        return new PersonId(id);
    }
}

namespace TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

public interface IStronglyTypedId
{
    public Guid Value { get; }
}
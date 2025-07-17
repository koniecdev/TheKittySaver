namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Specifications;

public interface IValidPhoneNumberSpecification
{
    bool IsSatisfiedBy(string phoneNumber);
}

public interface IPhoneNumberNormalizer
{
    string Normalize(string phoneNumber);
}
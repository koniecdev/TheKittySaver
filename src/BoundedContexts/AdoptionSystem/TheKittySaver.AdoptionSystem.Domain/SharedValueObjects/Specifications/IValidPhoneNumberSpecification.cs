namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Specifications;

public interface IValidPhoneNumberSpecification
{
    bool IsSatisfiedBy(string phoneNumber);
}

public interface IPhoneNumberNormalizer
{
    string Normalize(string phoneNumber);
}
namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Specifications;

public interface IValidPhoneNumberSpecification
{
    bool IsSatisfiedBy(string phoneNumber);
}
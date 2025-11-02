namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

public interface IValidPhoneNumberSpecification
{
    bool IsSatisfiedBy(string phoneNumber);
}
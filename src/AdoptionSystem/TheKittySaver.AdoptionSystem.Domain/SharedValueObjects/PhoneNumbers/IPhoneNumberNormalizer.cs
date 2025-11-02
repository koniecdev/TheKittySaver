namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

public interface IPhoneNumberNormalizer
{
    string Normalize(string phoneNumber);
}
namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Specifications;

public interface IPhoneNumberNormalizer
{
    string Normalize(string phoneNumber);
}
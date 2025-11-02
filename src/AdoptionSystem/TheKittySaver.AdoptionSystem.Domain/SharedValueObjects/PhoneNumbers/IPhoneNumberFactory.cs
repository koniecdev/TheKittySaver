using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

public interface IPhoneNumberFactory
{
    Result<PhoneNumber> Create(string value);
}
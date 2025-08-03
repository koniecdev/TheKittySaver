using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Factories;

public interface IPhoneNumberFactory
{
    Result<PhoneNumber> Create(string value);
}
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

internal sealed class PhoneNumberFactory : IPhoneNumberFactory
{
    private readonly IValidPhoneNumberSpecification _specification;
    private readonly IPhoneNumberNormalizer _phoneNumberNormalizer;

    public PhoneNumberFactory(
        IValidPhoneNumberSpecification specification,
        IPhoneNumberNormalizer phoneNumberNormalizer)
    {
        _specification = specification;
        _phoneNumberNormalizer = phoneNumberNormalizer;
    }

    public Result<PhoneNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<PhoneNumber>(
                DomainErrors.PhoneNumberValueObject.NullOrEmpty);
        }

        value = value.Trim();

        if (value.Length > PhoneNumber.MaxLength)
        {
            return Result.Failure<PhoneNumber>(
                DomainErrors.PhoneNumberValueObject.LongerThanAllowed);
        }

        if (!_specification.IsSatisfiedBy(value))
        {
            return Result.Failure<PhoneNumber>(
                DomainErrors.PhoneNumberValueObject.InvalidFormat);
        }

        value = _phoneNumberNormalizer.Normalize(value);
        PhoneNumber instance = PhoneNumber.CreateUnsafe(value);

        return Result.Success(instance);
    }
}

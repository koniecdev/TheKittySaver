using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Specifications;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Factories;

public sealed class PhoneNumberFactory : IPhoneNumberFactory
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
                DomainErrors.PersonEntity.PhoneNumberProperty.NullOrEmpty);
        }
        
        string trimmedValue = value.Trim();
        
        if (trimmedValue.Length > PhoneNumber.MaxLength)
        {
            return Result.Failure<PhoneNumber>(
                DomainErrors.PersonEntity.PhoneNumberProperty.LongerThanAllowed);
        }
        
        if (!_specification.IsSatisfiedBy(trimmedValue))
        {
            return Result.Failure<PhoneNumber>(
                DomainErrors.PersonEntity.PhoneNumberProperty.InvalidFormat);
        }
        
        string normalized = _phoneNumberNormalizer.Normalize(trimmedValue);
        PhoneNumber instance = PhoneNumber.CreateUnsafe(normalized);
        
        return Result.Success(instance);
    }
}
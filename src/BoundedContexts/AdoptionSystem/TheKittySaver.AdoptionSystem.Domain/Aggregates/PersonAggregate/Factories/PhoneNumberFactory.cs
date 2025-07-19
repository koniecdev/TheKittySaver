using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Specifications;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Factories;

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
        Result<PhoneNumber> result = Result.Create(value, DomainErrors.PersonEntity.PhoneNumberProperty.NullOrEmpty)
            .Ensure(v => !string.IsNullOrWhiteSpace(v), 
                DomainErrors.PersonEntity.PhoneNumberProperty.NullOrEmpty)
            .Ensure(v => v.Length <= PhoneNumber.MaxLength, 
                DomainErrors.PersonEntity.PhoneNumberProperty.LongerThanAllowed)
            .Ensure(v => _specification.IsSatisfiedBy(v),
                DomainErrors.PersonEntity.PhoneNumberProperty.InvalidFormat)
            .Map(v =>
            {
                string normalized = _phoneNumberNormalizer.Normalize(v);
                PhoneNumber instance = PhoneNumber.CreateUnsafe(normalized);
                return instance;
            });

        return result;
    }
}
using PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Specifications;

namespace TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

public sealed class LibPhoneNumberSpecification : IValidPhoneNumberSpecification, IPhoneNumberNormalizer
{
    private readonly PhoneNumberUtil _phoneUtil = PhoneNumberUtil.GetInstance();
    
    public bool IsSatisfiedBy(string phoneNumber)
    {
        try
        {
            PhoneNumber? parsed = _phoneUtil.Parse(phoneNumber, "PL");
            bool isValid = _phoneUtil.IsValidNumber(parsed);
            return isValid;
        }
        catch
        {
            return false;
        }
    }
    
    public string Normalize(string phoneNumber)
    {
        PhoneNumber? parsed = _phoneUtil.Parse(phoneNumber, "PL");
        string normalized = _phoneUtil.Format(parsed, PhoneNumberFormat.E164);
        return normalized;
    }
}
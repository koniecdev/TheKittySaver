// Infrastructure/PhoneValidation/LibPhoneNumberValidator.cs

using PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Specifications;

namespace TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

public sealed class LibPhoneNumberValidator : IValidPhoneNumberSpecification
{
    private readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public bool IsSatisfiedBy(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return false;
        }
        
        try 
        {
            PhoneNumber parsedNumber;
            
            if (phoneNumber.StartsWith('+') || phoneNumber.StartsWith("00"))
            {
                parsedNumber = _phoneNumberUtil.Parse(phoneNumber, null);
            }
            else
            {
                parsedNumber = _phoneNumberUtil.Parse(phoneNumber, "PL");
            }
            
            bool isValid = _phoneNumberUtil.IsValidNumber(parsedNumber);

            return isValid;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}
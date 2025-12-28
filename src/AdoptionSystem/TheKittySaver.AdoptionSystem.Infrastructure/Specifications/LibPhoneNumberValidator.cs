using PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using PhoneNumber = PhoneNumbers.PhoneNumber;

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
            PhoneNumber parsedNumber = phoneNumber.StartsWith('+') || phoneNumber.StartsWith("00", StringComparison.Ordinal)
                ? _phoneNumberUtil.Parse(phoneNumber, null)
                : _phoneNumberUtil.Parse(phoneNumber, "PL");

            bool isValid = _phoneNumberUtil.IsValidNumber(parsedNumber);

            return isValid;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}

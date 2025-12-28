using PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using PhoneNumber = PhoneNumbers.PhoneNumber;

namespace TheKittySaver.AdoptionSystem.Infrastructure.Specifications;

public class LibPhoneNumberNormalizer : IPhoneNumberNormalizer
{
    private readonly PhoneNumberUtil _phoneNumberUtil = PhoneNumberUtil.GetInstance();

    public string Normalize(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            return phoneNumber;
        }

        try
        {
            PhoneNumber parsedNumber = phoneNumber.StartsWith('+') || phoneNumber.StartsWith("00", StringComparison.Ordinal)
                ? _phoneNumberUtil.Parse(phoneNumber, null)
                : _phoneNumberUtil.Parse(phoneNumber, "PL");

            string normalized = _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.E164);
            return normalized;
        }
        catch (NumberParseException)
        {
            return phoneNumber;
        }
    }
}

// Infrastructure/PhoneValidation/LibPhoneNumberNormalizer.cs

using PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Specifications;

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
            PhoneNumber parsedNumber;
            
            // Ta sama logika parsowania co w walidatorze
            if (phoneNumber.StartsWith('+') || phoneNumber.StartsWith("00"))
            {
                parsedNumber = _phoneNumberUtil.Parse(phoneNumber, null);
            }
            else
            {
                parsedNumber = _phoneNumberUtil.Parse(phoneNumber, "PL");
            }
            
            // Normalizuj do formatu E164 (np. +48123456789)
            // Ten format jest uniwersalny i jednoznaczny
            string normalized = _phoneNumberUtil.Format(parsedNumber, PhoneNumberFormat.E164);
            return normalized;
        }
        catch (NumberParseException)
        {
            return phoneNumber;
        }
    }
}
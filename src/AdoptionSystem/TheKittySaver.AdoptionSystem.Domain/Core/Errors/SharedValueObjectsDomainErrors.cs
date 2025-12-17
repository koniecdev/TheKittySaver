using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class EmailValueObject
    {
        public static Error NullOrEmpty
            => Required(
                nameof(Email),
                nameof(Email.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(
                nameof(Email),
                nameof(Email.Value),
                Email.MaxLength);

        public static Error InvalidFormat
            => BadFormat(
                nameof(Email),
                nameof(Email.Value));
    }

    public static class PhoneNumberValueObject
    {
        public static Error NullOrEmpty
            => Required(
                nameof(PhoneNumber),
                nameof(PhoneNumber.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(
                nameof(PhoneNumber),
                nameof(PhoneNumber.Value),
                PhoneNumber.MaxLength);

        public static Error InvalidFormat
            => BadFormat(
                nameof(PhoneNumber),
                nameof(PhoneNumber.Value));
    }

    public static class AddressCityValueObject
    {
        public static Error NullOrEmpty
            => Required(
                nameof(AddressCity),
                nameof(AddressCity.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(
                nameof(AddressCity),
                nameof(AddressCity.Value),
                AddressCity.MaxLength);
    }

    public static class AddressRegionValueObject
    {
        public static Error NullOrEmpty
            => Required(
                nameof(AddressRegion),
                nameof(AddressRegion.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(
                nameof(AddressRegion),
                nameof(AddressRegion.Value),
                AddressRegion.MaxLength);
    }
    
    public static class AddressPostalCodeValueObject
    {
        public static Error NullOrEmpty
            => Required(
                nameof(AddressPostalCode),
                nameof(AddressPostalCode.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(
                nameof(AddressPostalCode),
                nameof(AddressPostalCode.Value),
                AddressPostalCode.MaxLength);
    }

    public static class AddressLineValueObject
    {
        public static Error NullOrEmpty
            => Required(
                nameof(AddressLine),
                nameof(AddressLine.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(
                nameof(AddressLine),
                nameof(AddressLine.Value),
                AddressLine.MaxLength);
    }
    
    public static class AddressConsistency
    {
        public const string PostalCodeRegionMismatchCode = "AddressConsistency.PostalCodeRegionMismatch";
        public const string InvalidPostalCodeFormatCode = "AddressConsistency.InvalidPostalCodeFormat";
        public const string InvalidRegionCode = "AddressConsistency.InvalidRegion";
        public const string PostalCodeRequiredCode = "AddressConsistency.PostalCodeRequired";
        public const string UnknownPostalCodePrefixCode = "AddressConsistency.UnknownPostalCodePrefix";

        public static Error PostalCodeRegionMismatch(string postalCode, string region)
            => new(
                PostalCodeRegionMismatchCode,
                $"Postal code '{postalCode}' does not match the specified region '{region}'.",
                TypeOfError.Validation);

        public static Error InvalidPostalCodeFormat(string postalCode)
            => new(
                InvalidPostalCodeFormatCode,
                $"Postal code '{postalCode}' has invalid format for the specified country.",
                TypeOfError.Validation);

        public static Error InvalidRegion(string region)
            => new(
                InvalidRegionCode,
                $"Region '{region}' is not a valid region for the specified country.",
                TypeOfError.Validation);

        public static Error PostalCodeRequired
            => new(
                PostalCodeRequiredCode,
                "Postal code is required.",
                TypeOfError.Validation);

        public static Error UnknownPostalCodePrefix(string prefix)
            => new(
                UnknownPostalCodePrefixCode,
                $"Postal code prefix '{prefix}' is not recognized for the specified country.",
                TypeOfError.Validation);
    }
    
    public static class ClaimedAtValueObject
    {
        public static Error CannotBeInThePast
            => DateIsInThePast(
                nameof(ClaimedAt),
                nameof(ClaimedAt.Value));
    }
    
    public static class ArchivedAtValueObject
    {
        public static Error CannotBeInThePast
            => DateIsInThePast(
                nameof(ArchivedAt),
                nameof(ArchivedAt.Value));
    }

    public static class PublishedAtValueObject
    {
        public static Error CannotBeInThePast
            => DateIsInThePast(
                nameof(PublishedAt),
                nameof(PublishedAt.Value));
    }
}

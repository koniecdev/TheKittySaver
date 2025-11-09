using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class EmailValueObject
    {
        public static Error NullOrEmpty
            => Required(nameof(Email), nameof(Email.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(nameof(Email), nameof(Email.Value), Email.MaxLength);

        public static Error InvalidFormat
            => BadFormat(nameof(Email), nameof(Email.Value));
    }

    public static class PhoneNumberValueObject
    {
        public static Error NullOrEmpty
            => Required(nameof(PhoneNumber), nameof(PhoneNumber.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(nameof(PhoneNumber), nameof(PhoneNumber.Value), PhoneNumber.MaxLength);

        public static Error InvalidFormat
            => BadFormat(nameof(PhoneNumber), nameof(PhoneNumber.Value));
    }

    public static class AddressCityValueObject
    {
        public static Error NullOrEmpty
            => Required(nameof(AddressCity), nameof(AddressCity.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(nameof(AddressCity), nameof(AddressCity.Value), AddressCity.MaxLength);
    }

    public static class AddressRegionValueObject
    {
        public static Error NullOrEmpty
            => Required(nameof(AddressRegion), nameof(AddressRegion.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(nameof(AddressRegion), nameof(AddressRegion.Value), AddressRegion.MaxLength);
    }

    public static class AddressLineValueObject
    {
        public static Error NullOrEmpty
            => Required(nameof(AddressLine), nameof(AddressLine.Value));

        public static Error LongerThanAllowed
            => TooManyCharacters(nameof(AddressLine), nameof(AddressLine.Value), AddressLine.MaxLength);
    }
}
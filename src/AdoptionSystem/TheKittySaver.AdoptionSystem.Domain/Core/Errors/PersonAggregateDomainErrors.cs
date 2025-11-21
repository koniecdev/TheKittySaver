using PersonEntity = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities.Person;
using AddressEntity = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities.Address;
using UsernameValueObject = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects.Username;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class Person
    {
        public static Error NotFound(PersonId id)
            => HasNotBeenFound(nameof(Person), id.Value);

        public static class IdentityId
        {
            public static Error AlreadyHasBeenSet
                => StateConflict(
                    nameof(Person),
                    nameof(PersonEntity.IdentityId),
                    "IdentityId has been set already.",
                    "AlreadyHasBeenSet");
        }

        public static class Username
        {
            public static Error NullOrEmpty
                => Required(nameof(Person), nameof(PersonEntity.Username));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(Person),
                    nameof(PersonEntity.Username),
                    UsernameValueObject.MaxLength);
        }

        public static Error EmailAlreadyTaken(Email email)
            => AlreadyHasBeenTaken(nameof(Person), nameof(PersonEntity.Email), email);

        public static Error PhoneNumberAlreadyTaken(PhoneNumber phoneNumber)
            => AlreadyHasBeenTaken(nameof(Person), nameof(PersonEntity.PhoneNumber), phoneNumber);
    }

    public static class Address
    {
        public static Error NotFound(AddressId addressId)
            => HasNotBeenFound(nameof(Address), addressId.Value);

        public static class Name
        {
            public static Error NullOrEmpty
                => Required(nameof(Address), nameof(AddressEntity.Name));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(Address), nameof(AddressEntity.Name), AddressName.MaxLength);
        }

        public static Error NameAlreadyTaken(AddressName name)
            => AlreadyHasBeenTaken(nameof(Address), nameof(AddressEntity.Name), name);
    }
}

using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using UsernameValueObject = TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects.Username;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class PersonEntity
    {
        public static Error NotFound(PersonId id)
            => HasNotBeenFound(nameof(PersonEntity), id.Value);

        public static Error NotFoundByIdentityId(IdentityId id)
            => HasNotBeenFound(nameof(PersonEntity), id.Value);

        public static Error IsArchived(PersonId id)
            => InvalidOperation(
                nameof(PersonEntity),
                $"Person with ID '{id.Value}' is archived and cannot be modified.",
                "IsArchived");

        public static Error IsNotArchived(PersonId id)
            => InvalidOperation(
                nameof(PersonEntity),
                $"Person with ID '{id.Value}' is not archived.",
                "IsNotArchived");

        public static Error HasRelatedEntities(PersonId id)
            => InvalidDeleteOperation(
                nameof(PersonEntity),
                $"Cannot delete Person with ID '{id.Value}' because it has related Cats or Adoption Announcements. Archive instead or delete related entities first.",
                "HasRelatedEntities");

        public static class UsernameProperty
        {
            public static Error NullOrEmpty
                => Required(nameof(PersonEntity), nameof(Person.Username));

            public static Error LongerThanAllowed
                => TooManyCharacters(
                    nameof(PersonEntity),
                    nameof(Person.Username),
                    UsernameValueObject.MaxLength);
        }

        public static Error EmailAlreadyTaken(Email email)
            => AlreadyHasBeenTaken(nameof(PersonEntity), nameof(Person.Email), email);

        public static Error PhoneNumberAlreadyTaken(PhoneNumber phoneNumber)
            => AlreadyHasBeenTaken(nameof(PersonEntity), nameof(Person.PhoneNumber), phoneNumber);
    }

    public static class AddressEntity
    {
        public static Error NotFound(AddressId addressId)
            => HasNotBeenFound(nameof(AddressEntity), addressId.Value);

        public static class NameProperty
        {
            public static Error NullOrEmpty
                => Required(nameof(AddressEntity), nameof(Address.Name));

            public static Error LongerThanAllowed
                => TooManyCharacters(nameof(AddressEntity), nameof(Address.Name), AddressName.MaxLength);
        }

        public static Error NameAlreadyTaken(AddressName name)
            => AlreadyHasBeenTaken(nameof(AddressEntity), nameof(Address.Name), name);
    }
}

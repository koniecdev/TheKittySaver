using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class PersonAggregatePersonEntity
    {
        public static Error NotFound(PersonId id) 
            => HasNotBeenFound(
                nameof(Person),
                id.Value);
        public static class IdentityIdProperty
        {
            public static Error AlreadyHasBeenSet 
                => new(
                    "Person.IdentityId.AlreadyHasBeenSet", 
                    "IdentityId has been set already.");
        }
        
        public static class EmailProperty
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(Person),
                    nameof(Person.Email));
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Person),
                    nameof(Person.Email),
                    Email.MaxLength);
            public static Error InvalidFormat 
                => BadFormat(
                    nameof(Person), 
                    nameof(Person.Email));
            public static Error AlreadyTaken(Email email)
                => AlreadyHasBeenTaken(
                    nameof(Person),
                    nameof(Person.Email),
                    email);
        }
        
        public static class UsernameProperty
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(Person),
                    nameof(Person.Username));
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Person),
                    nameof(Person.Username),
                    Username.MaxLength);
        }
        
        public static class PhoneNumberProperty
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(Person),
                    nameof(Person.PhoneNumber));
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Person),
                    nameof(Person.PhoneNumber), PhoneNumber.MaxLength);
            public static Error InvalidFormat 
                => BadFormat(
                    nameof(Person),
                    nameof(Person.PhoneNumber));
            public static Error AlreadyTaken(PhoneNumber phoneNumber)
                => AlreadyHasBeenTaken(
                    nameof(Person),
                    nameof(Person.PhoneNumber),
                    phoneNumber);
        }
    }
}

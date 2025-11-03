using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
    public static class PersonEntity
    {
        public static Error NotFound(PersonId id) 
            => HasNotBeenFound(
                nameof(PersonEntity),
                id.Value);
        
        public static class IdentityId
        {
            public static Error AlreadyHasBeenSet 
                => new(
                    "Person.IdentityId.AlreadyHasBeenSet", 
                    "IdentityId has been set already.");
        }
        
        public static class UsernameValueObject
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(PersonEntity),
                    nameof(Person.Username));
            
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(PersonEntity),
                    nameof(Person.Username),
                    Username.MaxLength);
        }
        
        public static class EmailValueObject
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(Person),
                    nameof(Person.Email));
        
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Person),
                    nameof(Person.Email),
                    SharedValueObjects.Email.MaxLength);
        
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
    
        public static class PhoneNumberValueObject
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(Person),
                    nameof(Person.PhoneNumber));
        
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Person),
                    nameof(Person.PhoneNumber), 
                    SharedValueObjects.PhoneNumbers.PhoneNumber.MaxLength);
        
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
    
    public static class PersonAddressEntity
    {
        public static Error NotFound(AddressId addressId) 
            => HasNotBeenFound(
                nameof(PersonAddressEntity),
                addressId.Value);
        
        public static class NameValueObject
        {
            public static Error AlreadyTaken(AddressName name)
                => AlreadyHasBeenTaken(
                    nameof(PersonAddressEntity),
                    nameof(Address.Name),
                    name);
            
            public static Error NullOrEmpty
                => Required(
                    nameof(PersonAddressEntity),
                    nameof(Address.Name));
            
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(PersonAddressEntity),
                    nameof(Address.Name),
                    AddressName.MaxLength);
        }
        
        public static class RegionValueObject
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(PersonAddressEntity),
                    nameof(Address.Region));
            
            public static Error LongerThanAllowed
                => TooLong(
                    nameof(PersonAddressEntity),
                    nameof(Address.Region),
                    AddressRegion.MaxLength);
        }

        public static class CityValueObject
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(PersonAddressEntity),
                    nameof(Address.City));
            
            public static Error LongerThanAllowed
                => TooLong(
                    nameof(PersonAddressEntity),
                    nameof(Address.City),
                    AddressCity.MaxLength);
        }
        
        public static class LineValueObject
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(PersonAddressEntity),
                    nameof(Address.Line));
            
            public static Error LongerThanAllowed
                => TooLong(
                    nameof(PersonAddressEntity),
                    nameof(Address.Line),
                    AddressLine.MaxLength);
        }
    }
}
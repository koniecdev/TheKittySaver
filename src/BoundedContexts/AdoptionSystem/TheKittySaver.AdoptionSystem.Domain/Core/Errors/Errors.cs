using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static class DomainErrors
{
    private static Error Required(string entity, string property) 
        => new($"{entity}.{property}.NullOrEmpty",
            $"The {property.ToLower()} is required.");

    private static Error TooLong(string entity, string property, int maxLength) 
        => new($"{entity}.{property}.LongerThanAllowed", 
            $"The {property.ToLower()} exceeds maximum length of {maxLength}.");

    private static Error BadFormat(string entity, string property) 
        => new($"{entity}.{property}.InvalidFormat", 
            $"The {property.ToLower()} format is invalid.");
    
    private static Error ValueInvalid(string entity, string property) 
        => new($"{entity}.{property}.InvalidValue",
            $"The {property.ToLower()} value is invalid.");

    
    public static class PersonEntity
    {
        public static class IdentityIdProperty
        {
            public static Error AlreadyHasBeenSet 
                => new("Person.IdentityId.AlreadyHasBeenSet", "IdentityId has been set already.");
        }
        
        public static class EmailProperty
        {
            public static Error NullOrEmpty => Required(nameof(Person), nameof(Person.Email));
            public static Error LongerThanAllowed => TooLong(nameof(Person), nameof(Person.Email), Email.MaxLength);
            public static Error InvalidFormat => BadFormat(nameof(Person), nameof(Person.Email));
        }
        
        public static class UsernameProperty
        {
            public static Error NullOrEmpty => Required(nameof(Person), nameof(Person.Username));
            public static Error LongerThanAllowed => TooLong(nameof(Person), nameof(Person.Username), Username.MaxLength);
        }
        
        public static class PhoneNumberProperty
        {
            public static Error NullOrEmpty => Required(nameof(Person), nameof(Person.PhoneNumber));
            public static Error LongerThanAllowed => TooLong(nameof(Person), nameof(Person.Username), PhoneNumber.MaxLength);
        }
    }

    public static class PolishAddressEntity
    {
        public static class NameProperty
        {
            public static Error NullOrEmpty => Required(nameof(PolishAddress), nameof(PolishAddress.Name));
            public static Error LongerThanAllowed => Required(nameof(PolishAddress), nameof(PolishAddress.Name));
        }
        
        public static class VoivodeshipProperty
        {
            public static Error InvalidValue => ValueInvalid(nameof(PolishAddress), nameof(PolishAddress.Voivodeship));
        }
        
        public static class CountyProperty
        {
            public static Error InvalidValue => ValueInvalid(nameof(PolishAddress), nameof(PolishAddress.County));
        }
        
         public static class ZipCodeProperty
        {
            public static Error NullOrEmpty => Required(nameof(PolishAddress), nameof(PolishAddress.ZipCode));
            public static Error InvalidFormat => BadFormat(nameof(PolishAddress), nameof(PolishAddress.ZipCode));
        }
        
        public static class CityProperty
        {
            public static Error NullOrEmpty 
                => Required(nameof(PolishAddress), nameof(PolishAddress.City));
            public static Error LongerThanAllowed 
                => TooLong(nameof(PolishAddress), nameof(PolishAddress.City), City.MaxLength);
        }
        
        public static class StreetProperty
        {
            public static Error NullOrEmpty 
                => Required(nameof(PolishAddress), nameof(PolishAddress.Street));
            public static Error LongerThanAllowed 
                => TooLong(nameof(PolishAddress), nameof(PolishAddress.Street), Street.MaxLength);
        }
        
        public static class BuildingNumberProperty
        {
            public static Error NullOrEmpty 
                => Required(nameof(PolishAddress), nameof(PolishAddress.BuildingNumber));
            public static Error LongerThanAllowed => TooLong(nameof(PolishAddress), nameof(PolishAddress.BuildingNumber), BuildingNumber.MaxLength);
            public static Error InvalidFormat 
                => BadFormat(nameof(PolishAddress), nameof(PolishAddress.BuildingNumber));
        }
        
        public static class ApartmentNumberProperty
        {
            public static Error NullOrEmpty 
                => Required(nameof(PolishAddress), nameof(PolishAddress.ApartmentNumber));
            public static Error LongerThanAllowed 
                => TooLong(nameof(PolishAddress), nameof(PolishAddress.ApartmentNumber), ApartmentNumber.MaxLength);
            public static Error InvalidFormat 
                => BadFormat(nameof(PolishAddress), nameof(PolishAddress.ApartmentNumber));
        }
    }
}
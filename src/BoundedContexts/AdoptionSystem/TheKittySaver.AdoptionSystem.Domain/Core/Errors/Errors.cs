using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using Email = TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Email;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static class DomainErrors
{
    private static Error HasNotBeenFound(string entity, Guid id) 
        => new($"{entity}.NotFound",
            $"The {entity.ToLower()} with id: {id} has not been found.");
    
    private static Error Required(string entity, string property) 
        => new($"{entity}.{property}.NullOrEmpty",
            $"The {property.ToLower()} is required.");
    
    private static Error AlreadyHasBeenTaken(string entity, string property, object alreadyTakenValue) 
        => new($"{entity}.{property}.AlreadyTaken",
            $"The {property.ToLower()} value '{alreadyTakenValue}' is already taken.");

    private static Error TooLong(string entity, string property, int maxLength) 
        => new($"{entity}.{property}.LongerThanAllowed", 
            $"The {property.ToLower()} exceeds maximum length of {maxLength}.");

    private static Error BadFormat(string entity, string property) 
        => new($"{entity}.{property}.InvalidFormat", 
            $"The {property.ToLower()} format is invalid.");
    
    private static Error ValueInvalid(string entity, string property) 
        => new($"{entity}.{property}.InvalidValue",
            $"The {property.ToLower()} value is invalid.");
    
    private static Error MustBeEmpty(string entity, string property, string propertyThatIsEmpty) 
        => new($"{entity}.{property}.MustBeEmpty",
            $"The {property.ToLower()} must be empty, when {propertyThatIsEmpty.ToLower()} is empty");
    
    private static Error BelowValue<T>(string entity, string property, T actualValue, T minimalValue) where T : struct
        => new($"{entity}.{property}.BelowValue", 
            $"The {property.ToLower()} has been set with value '{actualValue}', and it is below the minimum required value '{minimalValue}'.");
    
    private static Error AboveValue<T>(string entity, string property, T actualValue, T maximumValue) where T : struct
        => new($"{entity}.{property}.AboveValue", 
            $"The {property.ToLower()} has been set with value '{actualValue}', and it is above the maximum required value '{maximumValue}'.");
    
    private static Error CustomMessage(string entity, string property, string message) 
        => new($"{entity}.{property}.InvalidValue", message);
    
    public static class PersonEntity
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
    
    public static class AddressEntity
    {
        public static Error NotFound(AddressId addressId) 
            => HasNotBeenFound(
                nameof(Address),
                addressId.Value);
        
        public static class NameProperty
        {
            public static Error AlreadyTaken(AddressName name)
                => AlreadyHasBeenTaken(
                    nameof(Address),
                    nameof(Address.Name),
                    name);
            public static Error NullOrEmpty
                => Required(
                    nameof(Address),
                    nameof(Address.Name));
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Address),
                    nameof(Address.Name),
                    AddressName.MaxLength);
        }
        
        public static class RegionProperty
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(Address),
                    nameof(Address.Region));
            public static Error LongerThanAllowed
                => TooLong(
                    nameof(Address),
                    nameof(Address.Region),
                    AddressRegion.MaxLength);
        }

        public static class CityProperty
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(Address),
                    nameof(Address.City));
            public static Error LongerThanAllowed
                => TooLong(
                    nameof(Address),
                    nameof(Address.City),
                    AddressLine.MaxLength);
        }
        
        public static class LineProperty
        {
            public static Error NullOrEmpty
                => Required(
                    nameof(Address),
                    nameof(Address.Line));
            public static Error LongerThanAllowed
                => TooLong(
                    nameof(Address),
                    nameof(Address.Line),
                    AddressLine.MaxLength);
        }
    }
    
    public static class CatEntity
    {
        public static class CatNameProperty
        {
            public static Error NullOrEmpty 
                => Required(
                    nameof(Cat),
                    nameof(Cat.Name));
            public static Error LongerThanAllowed 
                => TooLong(
                    nameof(Cat),
                    nameof(Cat.Name),
                    CatName.MaxLength);
        }
        
        public static class CatAgeProperty
        {
            public static Error BelowMinimalAllowedValue(int actualValue, int minimumValue) 
                => BelowValue(
                    nameof(Cat),
                    nameof(Cat.Age),
                    actualValue,
                    minimumValue);
            public static Error AboveMaximumAllowedValue(int actualValue, int maximumValue)
                => AboveValue(
                    nameof(Cat),
                    nameof(Cat.Age),
                    actualValue,
                    maximumValue);
        }
        
        public static class AdoptionHistoryProperty
        {
            public static Error CountTooLow
                => CustomMessage(nameof(Cat), nameof(Cat.AdoptionHistory.ReturnCount),
                    "If cat has been returned, then it must has been returned at least one time.");
            public static Error LastReturnTooFarInPast
                => CustomMessage(nameof(Cat), nameof(Cat.AdoptionHistory.LastReturnDate),
                    "Invalid last return date has been provided.");
            public static Error LastReturnReasonIsEmpty
                => Required(nameof(Cat), nameof(Cat.AdoptionHistory.LastReturnReason));
        }
        
        public static class ListingSourceProperty
        {
            public static Error SourceNameIsNullOrEmpty 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.ListingSource)}.{nameof(Cat.ListingSource.SourceName)}");
            
            public static Error SourceNameIsLongerThanAllowed 
                => TooLong(
                    nameof(Cat),
                    $"{nameof(Cat.ListingSource)}.{nameof(Cat.ListingSource.SourceName)}",
                    ListingSource.MaxSourceNameLength);
            
            public static Error TypeIsUnset 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.ListingSource.Type)}");
        }
        
        public static class SpecialNeedsStatusProperty
        {
            public static Error DescriptionIsNullOrEmpty 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.SpecialNeeds.Description)}");
            
            public static Error DescriptionIsLongerThanAllowed 
                => TooLong(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.SpecialNeeds.Description)}",
                    SpecialNeedsStatus.MaxDescriptionLength);
            
            public static Error SpecialNeedsSeverityIsUnset 
                => Required(
                    nameof(Cat),
                    $"{nameof(Cat.SpecialNeeds)}.{nameof(Cat.SpecialNeeds.SeverityType)}");
        }
    }
    
    public static class AdoptionPriorityScoreValueObject
    {
        public static Error BelowMinimalAllowedValue(decimal actualValue, decimal minimumValue) 
            => BelowValue(
                nameof(Cat),
                nameof(AdoptionPriorityScore),
                actualValue,
                minimumValue);
        public static Error AboveMaximumAllowedValue(decimal actualValue, decimal maximumValue)
            => AboveValue(
                nameof(Cat),
                nameof(AdoptionPriorityScore),
                actualValue,
                maximumValue);
    }
}
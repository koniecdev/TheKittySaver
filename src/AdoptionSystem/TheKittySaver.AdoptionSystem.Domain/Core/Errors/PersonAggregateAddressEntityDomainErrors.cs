using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Errors;

public static partial class DomainErrors
{
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
}
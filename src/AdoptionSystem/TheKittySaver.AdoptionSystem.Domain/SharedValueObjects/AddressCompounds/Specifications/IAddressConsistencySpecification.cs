using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;

public interface IAddressConsistencySpecification
{
    bool IsSatisfiedBy(CountryCode countryCode, string postalCode, string region);
}
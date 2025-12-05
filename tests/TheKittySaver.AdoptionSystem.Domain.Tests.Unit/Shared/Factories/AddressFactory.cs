using Bogus;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Extensions;
using TheKittySaver.AdoptionSystem.Infrastructure.Specifications;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Tests.Unit.Shared.Factories;

internal static class AddressFactory
{
    public static Address CreateRandom(
        Faker faker,
        PersonId? personId = null,
        bool replacePersonIdWithEmpty = false,
        bool replaceCountryCodeWithUnset = false,
        bool replaceNameWithNull = false,
        bool replaceRegionWithNull = false,
        bool replaceCityWithNull = false,
        bool includeLine = true)
    {
        PersonId thePersonId = personId ?? PersonId.New();

        Result<AddressName> nameResult = AddressName.Create(faker.Address.StreetName());
        nameResult.EnsureSuccess();

        const string regionValue = "Wielkopolskie";
        Result<AddressRegion> regionResult = AddressRegion.Create(regionValue);
        regionResult.EnsureSuccess();

        Result<AddressCity> cityResult = AddressCity.Create(faker.Address.City());
        cityResult.EnsureSuccess();

        const string postalCodeValue = "60-123";
        Result<AddressPostalCode> postalCodeResult = AddressPostalCode.Create(postalCodeValue);
        postalCodeResult.EnsureSuccess();

        Maybe<AddressLine> maybeLine = Maybe<AddressLine>.None;
        if (includeLine)
        {
            Result<AddressLine> lineResult = AddressLine.Create(faker.Address.StreetAddress());
            lineResult.EnsureSuccess();
            maybeLine = Maybe<AddressLine>.From(lineResult.Value);
        }

        IAddressConsistencySpecification specification = new PolandAddressConsistencySpecification();

        Result<Address> addressResult = Address.Create(
            specification: specification,
            personId: replacePersonIdWithEmpty ? PersonId.Empty : thePersonId,
            countryCode: replaceCountryCodeWithUnset ? CountryCode.Unset : CountryCode.PL,
            name: replaceNameWithNull ? null! : nameResult.Value,
            postalCode: postalCodeResult.Value,
            region: replaceRegionWithNull ? null! : regionResult.Value,
            city: replaceCityWithNull ? null! : cityResult.Value,
            maybeLine: maybeLine);
        addressResult.EnsureSuccess();

        return addressResult.Value;
    }

    public static AddressName CreateRandomName(Faker faker)
    {
        Result<AddressName> result = AddressName.Create(faker.Address.StreetName());
        result.EnsureSuccess();
        return result.Value;
    }

    public static AddressRegion CreateRandomRegion(Faker faker)
    {
        Result<AddressRegion> result = AddressRegion.Create(faker.Address.State());
        result.EnsureSuccess();
        return result.Value;
    }

    public static AddressCity CreateRandomCity(Faker faker)
    {
        Result<AddressCity> result = AddressCity.Create(faker.Address.City());
        result.EnsureSuccess();
        return result.Value;
    }

    public static AddressLine CreateRandomLine(Faker faker)
    {
        Result<AddressLine> result = AddressLine.Create(faker.Address.StreetAddress());
        result.EnsureSuccess();
        return result.Value;
    }

    public static AddressPostalCode CreateFixedPostalCode()
    {
        Result<AddressPostalCode> result = AddressPostalCode.Create("60-365");
        result.EnsureSuccess();
        return result.Value;
    }
}

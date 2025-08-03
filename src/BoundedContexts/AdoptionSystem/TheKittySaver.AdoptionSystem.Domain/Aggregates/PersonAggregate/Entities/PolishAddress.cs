using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using PolishZipCode = TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PolishZipCode;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class PolishAddress : Entity<PolishAddressId>
{
    public PersonId PersonId { get; }
    public AddressName Name { get; private set; }
    public PolandVoivodeship Voivodeship { get; private set; }
    public PolandCounty County { get; private set; }
    public PolishZipCode ZipCode { get; private set; }
    public City City { get; private set; }
    public Street? Street { get; private set; }
    public BuildingNumber? BuildingNumber { get; private set; }
    public ApartmentNumber? ApartmentNumber { get; private set; }

    public void UpdateName(AddressName name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
    }
    
    public Result UpdateAddress(
        PolandVoivodeship voivodeship,
        PolandCounty county,
        PolishZipCode zipCode,
        City city,
        Maybe<Street> maybeStreet,
        Maybe<BuildingNumber> maybeBuildingNumber,
        Maybe<ApartmentNumber> maybeApartmentNumber)
    {
        Ensure.HasValue(voivodeship);
        Ensure.HasValue(county);
        ArgumentNullException.ThrowIfNull(zipCode);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeStreet);
        ArgumentNullException.ThrowIfNull(maybeBuildingNumber);
        ArgumentNullException.ThrowIfNull(maybeApartmentNumber);

        Result addressHierarchyValidationResult =
            ValidateAddressHierarchy(maybeStreet, maybeBuildingNumber, maybeApartmentNumber);

        if (addressHierarchyValidationResult.IsFailure)
        {
            return addressHierarchyValidationResult;
        }

        Voivodeship = voivodeship;
        County = county;
        ZipCode = zipCode;
        City = city;
        Street = maybeStreet.MatchSafely(
            onSuccess: street => street,
            onFailure: () => null
        );
        BuildingNumber = maybeBuildingNumber.MatchSafely(
            onSuccess: buildingNumber => buildingNumber,
            onFailure: () => null
        );
        ApartmentNumber = maybeApartmentNumber.MatchSafely(
            onSuccess: apartmentNumber => apartmentNumber,
            onFailure: () => null
        );
        
        return Result.Success();
    }
    
    internal static Result<PolishAddress> Create(
        PersonId personId,
        AddressName name,
        PolandVoivodeship voivodeship,
        PolandCounty county,
        PolishZipCode zipCode,
        City city,
        Maybe<Street> maybeStreet,
        Maybe<BuildingNumber> maybeBuildingNumber,
        Maybe<ApartmentNumber> maybeApartmentNumber)
    {
        Ensure.NotEmpty(personId);
        ArgumentNullException.ThrowIfNull(name);
        Ensure.HasValue(voivodeship);
        Ensure.HasValue(county);
        ArgumentNullException.ThrowIfNull(zipCode);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeStreet);
        ArgumentNullException.ThrowIfNull(maybeBuildingNumber);
        ArgumentNullException.ThrowIfNull(maybeApartmentNumber);
        
        if (maybeStreet.HasNoValue && maybeBuildingNumber.HasValue)
        {
            return Result.Failure<PolishAddress>(DomainErrors.PolishAddressEntity.BuildingNumberProperty.MustBeEmptyWhenStreetIsEmpty);
        }
        if (maybeBuildingNumber.HasNoValue && maybeApartmentNumber.HasValue)
        {
            return Result.Failure<PolishAddress>(DomainErrors.PolishAddressEntity.ApartmentNumberProperty.MustBeEmptyWhenBuildingNumberIsEmpty);
        }

        PolishAddressId id = PolishAddressId.New();
        PolishAddress instance = new(
            id,
            personId,
            name,
            voivodeship,
            county,
            zipCode,
            city,
            maybeStreet.MatchSafely(
                onSuccess: street => street,
                onFailure: () => null
            ),
            maybeBuildingNumber.MatchSafely(
                onSuccess: buildingNumber => buildingNumber,
                onFailure: () => null
            ),
            maybeApartmentNumber.MatchSafely(
                onSuccess: apartmentNumber => apartmentNumber,
                onFailure: () => null
            ));
        return instance;
    }

    private static Result ValidateAddressHierarchy(
        Maybe<Street> maybeStreet,
        Maybe<BuildingNumber> maybeBuildingNumber,
        Maybe<ApartmentNumber> maybeApartmentNumber)
    {
        if (maybeStreet.HasNoValue && maybeBuildingNumber.HasValue)
        {
            return Result.Failure(DomainErrors.PolishAddressEntity.BuildingNumberProperty.MustBeEmptyWhenStreetIsEmpty);
        }
    
        if (maybeBuildingNumber.HasNoValue && maybeApartmentNumber.HasValue)
        {
            return Result.Failure(DomainErrors.PolishAddressEntity.ApartmentNumberProperty.MustBeEmptyWhenBuildingNumberIsEmpty);
        }
    
        return Result.Success();
    }
    
    private PolishAddress(
        PolishAddressId id,
        PersonId personId,
        AddressName name,
        PolandVoivodeship voivodeship,
        PolandCounty county,
        PolishZipCode zipCode,
        City city,
        Street? street,
        BuildingNumber? buildingNumber,
        ApartmentNumber? apartmentNumber) : base(id)
    {
        PersonId = personId;
        Name = name;
        Voivodeship = voivodeship;
        County = county;
        ZipCode = zipCode;
        City = city;
        Street = street;
        BuildingNumber = buildingNumber;
        ApartmentNumber = apartmentNumber;
    }
}
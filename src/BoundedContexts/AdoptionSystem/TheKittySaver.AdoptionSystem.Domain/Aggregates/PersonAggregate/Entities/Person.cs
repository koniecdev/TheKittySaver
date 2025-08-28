using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class Person : AggregateRoot<PersonId>
{
    private readonly List<Address> _addresses = [];
    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();
    
    public IdentityId IdentityId { get; private set; } = IdentityId.Empty;
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }

    public Result<PolishAddressId> AddAddress(
        CountryCode countryCode,
        AddressName name,
        AddressRegion region,
        City city,
        Maybe<AddressLine> maybeLine)
    {
        Ensure.HasValue(countryCode);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        
        if (IsAddressNameTaken(name))
        {
            return Result.Failure<PolishAddressId>(
                DomainErrors.PolishAddressEntity.NameProperty.AlreadyTaken(name));
        }
        
        Result<Address> createAddressResult = Address.Create(
            personId: Id,
            countryCode: countryCode,
            name: name,
            region: region,
            city: city,
            maybeLine: maybeLine);
        
        if (createAddressResult.IsFailure)
        {
            return Result.Failure<PolishAddressId>(createAddressResult.Error);
        }
        
        _addresses.Add(createAddressResult.Value);
        return Result.Success(createAddressResult.Value.Id);
    }

    public Result UpdateAddressName(AddressId addressId, AddressName updatedName)
    {
        Ensure.NotEmpty(addressId);
        ArgumentNullException.ThrowIfNull(updatedName);
        
        Maybe<Address> maybeAddress = GetAddressById(addressId);
        if (maybeAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.AddressEntity.NotFound(addressId));
        }

        if (IsAddressNameTaken(updatedName, addressId))
        {
            return Result.Failure(DomainErrors.PolishAddressEntity.NameProperty.AlreadyTaken(updatedName));
        }

        maybeAddress.Value.UpdateName(updatedName);
        
        return Result.Success();
    }
    
    public Result UpdatePolishAddressDetails(
        PolishAddressId id,
        PolandVoivodeship updatedVoivodeship,
        PolandCounty updatedCounty,
        PolishZipCode updatedZipCode,
        City updatedCity,
        Maybe<Street> updatedMaybeStreet,
        Maybe<BuildingNumber> updatedMaybeBuildingNumber,
        Maybe<ApartmentNumber> updatedMaybeApartmentNumber)
    {
        Ensure.NotEmpty(id);
        Ensure.HasValue(updatedVoivodeship);
        Ensure.HasValue(updatedCounty);
        ArgumentNullException.ThrowIfNull(updatedZipCode);
        ArgumentNullException.ThrowIfNull(updatedCity);
        ArgumentNullException.ThrowIfNull(updatedMaybeStreet);
        ArgumentNullException.ThrowIfNull(updatedMaybeBuildingNumber);
        ArgumentNullException.ThrowIfNull(updatedMaybeApartmentNumber);
        
        Maybe<PolishAddress> maybePolishAddress = GetPolishAddressById(id);
        if (maybePolishAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.PolishAddressEntity.NotFound(id));
        }

        Result updateResult = maybePolishAddress.Value.UpdateAddress(
            updatedVoivodeship,
            updatedCounty,
            updatedZipCode,
            updatedCity,
            updatedMaybeStreet,
            updatedMaybeBuildingNumber,
            updatedMaybeApartmentNumber);
        
        return updateResult;
    }
    
    public Result DeletePolishAddress(PolishAddressId id)
    {
        Ensure.NotEmpty(id);
        
        Maybe<PolishAddress> maybePolishAddress = GetPolishAddressById(id);
        if (maybePolishAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.PolishAddressEntity.NotFound(id));
        }

        _polishAddresses.Remove(maybePolishAddress.Value);
        return Result.Success();
    }
    
    public Result SetIdentityId(IdentityId identityId)
    {
        Ensure.NotEmpty(identityId);
        if (IdentityId != IdentityId.Empty)
        {
            return Result.Failure(DomainErrors.PersonEntity.IdentityIdProperty.AlreadyHasBeenSet);
        }

        IdentityId = identityId;
        return Result.Success();
    }
    
    internal Result UpdateUsername(Username username)
    {
        ArgumentNullException.ThrowIfNull(username);
        Username = username;
        return Result.Success();
    }
    
    internal Result UpdateEmail(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);
        Email = email;
        return Result.Success();
    }
    
    internal Result UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        PhoneNumber = phoneNumber;
        return Result.Success();
    }
    
    internal static Result<Person> Create(
        Username username,
        Email email,
        PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        
        PersonId id = PersonId.New();
        Person instance = new(id, username, email, phoneNumber);
        
        return Result.Success(instance);
    }

    private Person(
        PersonId id,
        Username username,
        Email email,
        PhoneNumber phoneNumber) : base(id)
    {
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    private bool IsAddressNameTaken(AddressName name, PolishAddressId? idToExcludeInSearch = null)
    {
        bool isTaken = _polishAddresses.Any(address => 
            address.Name == name
            && (idToExcludeInSearch is null || address.Id != idToExcludeInSearch.Value));
        return isTaken;
    }   
    
    private Maybe<Address> GetAddressById(AddressId id)
    {
        Maybe<Address> maybeAddress = _addresses.GetByIdOrDefault(id);
        return maybeAddress;
    } 
}
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class Person : AggregateRoot<PersonId>
{
    private readonly List<PolishAddress> _polishAddresses = [];
    public IReadOnlyList<PolishAddress> PolishAddresses => _polishAddresses.AsReadOnly();
    
    public IdentityId IdentityId { get; private set; } = IdentityId.Empty;
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }

    public Result<PolishAddressId> AddPolishAddress(
        AddressName name,
        PolandVoivodeship voivodeship,
        PolandCounty county,
        PolishZipCode zipCode,
        City city,
        Maybe<Street> maybeStreet,
        Maybe<BuildingNumber> maybeBuildingNumber,
        Maybe<ApartmentNumber> maybeApartmentNumber)
    {
        ArgumentNullException.ThrowIfNull(name);
        Ensure.HasValue(voivodeship);
        Ensure.HasValue(county);
        ArgumentNullException.ThrowIfNull(zipCode);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeStreet);
        ArgumentNullException.ThrowIfNull(maybeBuildingNumber);
        ArgumentNullException.ThrowIfNull(maybeApartmentNumber);
        
        if (IsAddressNameTaken(name))
        {
            return Result.Failure<PolishAddressId>(
                DomainErrors.PolishAddressEntity.NameProperty.AlreadyTaken(name));
        }
        
        Result<PolishAddress> createAddressResult = PolishAddress.Create(
            Id,
            name,
            voivodeship,
            county,
            zipCode,
            city,
            maybeStreet,
            maybeBuildingNumber,
            maybeApartmentNumber);
        
        if (createAddressResult.IsFailure)
        {
            return Result.Failure<PolishAddressId>(createAddressResult.Error);
        }
        
        _polishAddresses.Add(createAddressResult.Value);
        return createAddressResult.Value.Id;
    }

    public Result UpdatePolishAddressName(PolishAddressId id, AddressName updatedName)
    {
        Ensure.NotEmpty(id);
        ArgumentNullException.ThrowIfNull(updatedName);
        
        Maybe<PolishAddress> maybePolishAddress = GetPolishAddressById(id);
        if (maybePolishAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.PolishAddressEntity.NotFound(id));
        }

        if (IsAddressNameTaken(updatedName, id))
        {
            return Result.Failure(DomainErrors.PolishAddressEntity.NameProperty.AlreadyTaken(updatedName));
        }

        maybePolishAddress.Value.UpdateName(updatedName);
        
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
    
    public void UpdateUsername(Username username)
    {
        ArgumentNullException.ThrowIfNull(username);
        Username = username;
    }
    
    public void UpdateEmail(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);
        Email = email;
    }
    
    public void UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        PhoneNumber = phoneNumber;
    }
    
    public static Result<Person> Create(
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
            address.Name == name && 
            (idToExcludeInSearch is null || address.Id != idToExcludeInSearch.Value));
        return isTaken;
    }
    
    private Maybe<PolishAddress> GetPolishAddressById(PolishAddressId id)
    {
        PolishAddress? polishAddress = _polishAddresses.FirstOrDefault(x => x.Id == id);
        return polishAddress is not null
            ? Maybe<PolishAddress>.From(polishAddress)
            : Maybe<PolishAddress>.None;
    } 
}
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

    public Result<AddressId> AddAddress(
        CountryCode countryCode,
        AddressName name,
        AddressRegion region,
        AddressCity addressCity,
        Maybe<AddressLine> maybeLine)
    {
        Ensure.HasValue(countryCode);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(addressCity);
        
        if (IsAddressNameTaken(name, ValueMaybe<AddressId>.None()))
        {
            return Result.Failure<AddressId>(DomainErrors.AddressEntity.NameProperty.AlreadyTaken(name));
        }
        
        Result<Address> createAddressResult = Address.Create(
            personId: Id,
            countryCode: countryCode,
            name: name,
            region: region,
            city: addressCity,
            maybeLine: maybeLine);
        
        if (createAddressResult.IsFailure)
        {
            return Result.Failure<AddressId>(createAddressResult.Error);
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
            return Result.Failure(DomainErrors.AddressEntity.NameProperty.AlreadyTaken(updatedName));
        }

        Result updateNameResult = maybeAddress.Value.UpdateName(updatedName);
        
        return updateNameResult;
    }
    
    public Result UpdateAddressDetails(
        AddressId id,
        AddressRegion region,
        AddressCity addressCity,
        Maybe<AddressLine> maybeLine)
    {
        Ensure.NotEmpty(id);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(addressCity);
        
        Maybe<Address> maybeAddressThatWeWantToUpdate = GetAddressById(id);
        if (maybeAddressThatWeWantToUpdate.HasNoValue)
        {
            return Result.Failure(DomainErrors.AddressEntity.NotFound(id));
        }

        Result updateRegionResult = maybeAddressThatWeWantToUpdate.Value.UpdateRegion(region);
        if (updateRegionResult.IsFailure)
        {
            return updateRegionResult;
        }
        
        Result updateCityResult = maybeAddressThatWeWantToUpdate.Value.UpdateCity(addressCity);
        if (updateCityResult.IsFailure)
        {
            return updateCityResult;
        }
        
        Result updateLineResult = maybeAddressThatWeWantToUpdate.Value.UpdateLine(maybeLine);
        return updateLineResult.IsFailure 
            ? updateLineResult 
            : Result.Success();
    }
    
    public Result DeleteAddress(AddressId id)
    {
        Ensure.NotEmpty(id);
        
        Maybe<Address> maybeAddress = GetAddressById(id);
        if (maybeAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.AddressEntity.NotFound(id));
        }

        return _addresses.Remove(maybeAddress.Value)
            ? Result.Success() 
            : throw new InvalidOperationException(
                "Address was not found in the list of addresses even though it was supposed to be there.");
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

    private bool IsAddressNameTaken(AddressName name, ValueMaybe<AddressId> maybeIdToExcludeInSearch)
    {
        IEnumerable<Address> isAddressTakenQuery = _addresses;
        
        if (maybeIdToExcludeInSearch.HasValue)
        {
            isAddressTakenQuery = isAddressTakenQuery.Where(address => address.Id != maybeIdToExcludeInSearch.Value);
        }
        
        bool isTaken = isAddressTakenQuery.Any(address => address.Name == name);
        
        return isTaken;
    }   
    
    private Maybe<Address> GetAddressById(AddressId id)
    {
        Maybe<Address> maybeAddress = _addresses.GetByIdOrDefault(id);
        return maybeAddress;
    } 
}
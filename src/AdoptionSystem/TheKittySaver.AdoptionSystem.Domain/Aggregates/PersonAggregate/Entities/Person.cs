using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

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
        ArgumentNullException.ThrowIfNull(name);
        
        if (_addresses.Any(x=>x.Name == name))
        {
            return Result.Failure<AddressId>(DomainErrors.PersonAddressEntity.NameValueObject.AlreadyTaken(name));
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
        
        Maybe<Address> maybeAddress = _addresses.GetByIdOrDefault(addressId);
        if (maybeAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.PersonAddressEntity.NotFound(addressId));
        }

        if (maybeAddress.Value.Name != updatedName && _addresses.Any(x=>x.Name == updatedName))
        {
            return Result.Failure(DomainErrors.PersonAddressEntity.NameValueObject.AlreadyTaken(updatedName));
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
        ArgumentNullException.ThrowIfNull(maybeLine);
        
        Maybe<Address> maybeAddressThatWeWantToUpdate = _addresses.GetByIdOrDefault(id);
        if (maybeAddressThatWeWantToUpdate.HasNoValue)
        {
            return Result.Failure(DomainErrors.PersonAddressEntity.NotFound(id));
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
        
        Maybe<Address> maybeAddress = _addresses.GetByIdOrDefault(id);
        if (maybeAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.PersonAddressEntity.NotFound(id));
        }

        return _addresses.Remove(maybeAddress.Value)
            ? Result.Success()
            : Result.Failure(DomainErrors.DeletionCorruption(nameof(Address)));
    }
    
    public Result SetIdentityId(IdentityId identityId)
    {
        Ensure.NotEmpty(identityId);
        if (IdentityId != IdentityId.Empty)
        {
            return Result.Failure(DomainErrors.PersonEntity.IdentityId.AlreadyHasBeenSet);
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
}
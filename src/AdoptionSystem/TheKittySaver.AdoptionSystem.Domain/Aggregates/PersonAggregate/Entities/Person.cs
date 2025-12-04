// Domain/Aggregates/PersonAggregate/Entities/Person.cs

using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class Person : AggregateRoot<PersonId>
{
    private readonly List<Address> _addresses = [];

    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();
    public IdentityId IdentityId { get; }
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }

    public Result<AddressId> AddAddress(
        IAddressConsistencySpecification specification,
        CountryCode countryCode,
        AddressName name,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        Maybe<AddressLine> maybeLine,
        CreatedAt createdAt)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(postalCode);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeLine);
        ArgumentNullException.ThrowIfNull(createdAt);

        if (_addresses.Any(x => x.Name == name))
        {
            return Result.Failure<AddressId>(DomainErrors.AddressEntity.NameAlreadyTaken(name));
        }

        Result<Address> createAddressResult = Address.Create(
            specification: specification,
            personId: Id,
            countryCode: countryCode,
            name: name,
            postalCode: postalCode,
            region: region,
            city: city,
            maybeLine: maybeLine,
            createdAt: createdAt);

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
            return Result.Failure(DomainErrors.AddressEntity.NotFound(addressId));
        }

        if (maybeAddress.Value.Name != updatedName && _addresses.Any(x => x.Name == updatedName))
        {
            return Result.Failure(DomainErrors.AddressEntity.NameAlreadyTaken(updatedName));
        }

        Result updateNameResult = maybeAddress.Value.UpdateName(updatedName);
        return updateNameResult;
    }

    public Result UpdateAddressDetails(
        IAddressConsistencySpecification specification,
        AddressId addressId,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        Maybe<AddressLine> maybeLine)
    {
        ArgumentNullException.ThrowIfNull(specification);
        Ensure.NotEmpty(addressId);
        ArgumentNullException.ThrowIfNull(postalCode);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeLine);

        Maybe<Address> maybeAddress = _addresses.GetByIdOrDefault(addressId);
        if (maybeAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.AddressEntity.NotFound(addressId));
        }

        Result updateAddressDetailsResult = maybeAddress.Value.UpdateDetails(
            specification,
            postalCode,
            region,
            city,
            maybeLine);
        
        return updateAddressDetailsResult;
    }

    public Result DeleteAddress(AddressId addressId)
    {
        Ensure.NotEmpty(addressId);

        Maybe<Address> maybeAddress = _addresses.GetByIdOrDefault(addressId);
        if (maybeAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.AddressEntity.NotFound(addressId));
        }

        return _addresses.Remove(maybeAddress.Value)
            ? Result.Success()
            : Result.Failure(DomainErrors.DeletionCorruption(nameof(Address)));
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
        PhoneNumber phoneNumber,
        CreatedAt createdAt,
        IdentityId identityId)
    {
        Ensure.NotEmpty(identityId);
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(createdAt);

        PersonId id = PersonId.New();
        Person instance = new(id, username, email, phoneNumber, createdAt, identityId);

        return Result.Success(instance);
    }

    private Person(
        PersonId id,
        Username username,
        Email email,
        PhoneNumber phoneNumber,
        CreatedAt createdAt,
        IdentityId identityId) : base(id, createdAt)
    {
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
        IdentityId = identityId;
    }
}
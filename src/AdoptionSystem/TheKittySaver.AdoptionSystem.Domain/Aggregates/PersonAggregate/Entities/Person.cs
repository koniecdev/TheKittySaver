using System.Diagnostics.CodeAnalysis;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Extensions;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class Person : AggregateRoot<PersonId>, IArchivable
{
    private readonly List<Address> _addresses = [];

    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();
    public IdentityId IdentityId { get; }
    public Username Username { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public ArchivedAt? ArchivedAt { get; private set; }

    public Result<Address> AddAddress(
        IAddressConsistencySpecification specification,
        CountryCode countryCode,
        AddressName name,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        Maybe<AddressLine> maybeLine)
    {
        Ensure.IsValidNonDefaultEnum(countryCode);
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(postalCode);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeLine);
        if (IsArchived(out Result? failure))
        {
            return Result.Failure<Address>(failure.Error);
        }

        if (_addresses.Any(x => x.Name == name))
        {
            return Result.Failure<Address>(DomainErrors.AddressEntity.NameAlreadyTaken(name));
        }

        Result<Address> createAddressResult = Address.Create(
            specification: specification,
            personId: Id,
            countryCode: countryCode,
            name: name,
            postalCode: postalCode,
            region: region,
            city: city,
            maybeLine: maybeLine);

        if (createAddressResult.IsFailure)
        {
            return Result.Failure<Address>(createAddressResult.Error);
        }

        _addresses.Add(createAddressResult.Value);
        return Result.Success(createAddressResult.Value);
    }

    public Result UpdateAddressName(AddressId addressId, AddressName updatedName)
    {
        Ensure.NotEmpty(addressId);
        ArgumentNullException.ThrowIfNull(updatedName);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

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
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

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

    public Result RemoveAddress(AddressId addressId)
    {
        Ensure.NotEmpty(addressId);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        Maybe<Address> maybeAddress = _addresses.GetByIdOrDefault(addressId);
        if (maybeAddress.HasNoValue)
        {
            return Result.Failure(DomainErrors.AddressEntity.NotFound(addressId));
        }

        return _addresses.Remove(maybeAddress.Value)
            ? Result.Success()
            : Result.Failure(DomainErrors.DeletionCorruption(nameof(Address)));
    }

    public Result UpdateUsername(Username username)
    {
        ArgumentNullException.ThrowIfNull(username);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        Username = username;
        return Result.Success();
    }

    internal Result UpdateEmail(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        Email = email;
        return Result.Success();
    }

    internal Result UpdatePhoneNumber(PhoneNumber phoneNumber)
    {
        ArgumentNullException.ThrowIfNull(phoneNumber);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        PhoneNumber = phoneNumber;
        return Result.Success();
    }

    internal Result Archive(ArchivedAt archivedAt)
    {
        ArgumentNullException.ThrowIfNull(archivedAt);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        ArchivedAt = archivedAt;

        Result anonymizeResult = Anonymize();
        return anonymizeResult;
    }

    internal Result Unarchive()
    {
        if (ArchivedAt is null)
        {
            return Result.Failure(DomainErrors.PersonEntity.IsNotArchived(Id));
        }

        ArchivedAt = null;
        return Result.Success();
    }

    private Result Anonymize()
    {
        string randomGuid = Guid.NewGuid().ToString()[..8];

        Result<Username> userNameAnonymizedResult = Username.Create(randomGuid);
        if (userNameAnonymizedResult.IsFailure)
        {
            return userNameAnonymizedResult;
        }
        Username = userNameAnonymizedResult.Value;

        Result<Email> emailAnonymizedResult = Email.Create($"x{randomGuid}@x{randomGuid}.com");
        if (emailAnonymizedResult.IsFailure)
        {
            return emailAnonymizedResult;
        }
        Email = emailAnonymizedResult.Value;

        PhoneNumber = PhoneNumber.CreateUnsafe(randomGuid);

        _addresses.Clear();

        return Result.Success();
    }

    internal static Result<Person> Create(
        Username username,
        Email email,
        PhoneNumber phoneNumber,
        IdentityId identityId)
    {
        Ensure.NotEmpty(identityId);
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);

        PersonId id = PersonId.Create();
        Person instance = new(id, username, email, phoneNumber, identityId);

        return Result.Success(instance);
    }

    private Person(
        PersonId id,
        Username username,
        Email email,
        PhoneNumber phoneNumber,
        IdentityId identityId) : base(id)
    {
        Username = username;
        Email = email;
        PhoneNumber = phoneNumber;
        IdentityId = identityId;
    }

    private Person()
    {
        Username = null!;
        Email = null!;
        PhoneNumber = null!;
    }

    private bool IsArchived([NotNullWhen(true)] out Result? failure)
    {
        bool isArchived = ArchivedAt is not null;

        failure = isArchived
            ? Result.Failure(DomainErrors.PersonEntity.IsArchived(Id))
            : Result.Success();

        return isArchived;
    }
}

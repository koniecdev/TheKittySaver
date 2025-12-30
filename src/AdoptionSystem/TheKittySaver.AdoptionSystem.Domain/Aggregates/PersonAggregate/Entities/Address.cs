using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class Address : Entity<AddressId>
{
    public PersonId PersonId { get; }
    public CountryCode CountryCode { get; }
    public AddressName Name { get; private set; }
    public AddressPostalCode PostalCode { get; private set; }
    public AddressRegion Region { get; private set; }
    public AddressCity City { get; private set; }
    public AddressLine? Line { get; private set; }

    internal Result UpdateName(AddressName name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
        return Result.Success();
    }

    internal Result UpdateDetails(
        IAddressConsistencySpecification specification,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        Maybe<AddressLine> maybeLine)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(postalCode);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeLine);

        if (!specification.IsSatisfiedBy(CountryCode, postalCode.Value, region.Value, out Error? error))
        {
            return Result.Failure(error!);
        }

        PostalCode = postalCode;
        Region = region;
        City = city;
        Line = maybeLine.HasValue ? maybeLine.Value : null;

        return Result.Success();
    }

    internal static Result<Address> Create(
        IAddressConsistencySpecification specification,
        PersonId personId,
        CountryCode countryCode,
        AddressName name,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        Maybe<AddressLine> maybeLine)
    {
        ArgumentNullException.ThrowIfNull(specification);
        Ensure.NotEmpty(personId);
        Ensure.IsValidNonDefaultEnum(countryCode);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(postalCode);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(maybeLine);

        if (!specification.IsSatisfiedBy(countryCode, postalCode.Value, region.Value, out Error? error))
        {
            return Result.Failure<Address>(error!);
        }

        AddressId id = AddressId.Create();
        Address instance = new(
            id,
            personId,
            countryCode,
            name,
            postalCode,
            region,
            city,
            maybeLine.HasValue ? maybeLine.Value : null);

        return Result.Success(instance);
    }

    private Address(
        AddressId id,
        PersonId personId,
        CountryCode countryCode,
        AddressName name,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        AddressLine? line) : base(id)
    {
        PersonId = personId;
        CountryCode = countryCode;
        Name = name;
        PostalCode = postalCode;
        Region = region;
        City = city;
        Line = line;
    }

    private Address()
    {
        Name = null!;
        PostalCode = null!;
        Region = null!;
        City = null!;
    }
}

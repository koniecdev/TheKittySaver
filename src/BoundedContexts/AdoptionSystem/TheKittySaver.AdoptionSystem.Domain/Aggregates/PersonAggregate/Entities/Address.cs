using PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Enums;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;

public sealed class Address : Entity<AddressId>
{
    public PersonId PersonId { get; }
    public CountryCode CountryCode { get; }
    public AddressName Name { get; private set; }
    public AddressRegion Region { get; private set; }
    public AddressCity City { get; private set; }
    public AddressLine? Line { get; private set; }

    internal Result UpdateName(AddressName name)
    {
        ArgumentNullException.ThrowIfNull(name);
        Name = name;
        return Result.Success();
    }

    internal Result UpdateRegion(AddressRegion region)
    {
        ArgumentNullException.ThrowIfNull(region);
        Region = region;
        return Result.Success();
    }

    internal Result UpdateCity(AddressCity city)
    {
        ArgumentNullException.ThrowIfNull(city);
        City = city;
        return Result.Success();
    }

    internal Result UpdateLine(Maybe<AddressLine> maybeLine)
    {
        Line = maybeLine.HasValue ? maybeLine.Value : null;
        return Result.Success();
    }
    
    internal static Result<Address> Create(
        PersonId personId,
        CountryCode countryCode,
        AddressName name,
        AddressRegion region,
        AddressCity city,
        Maybe<AddressLine> maybeLine)
    {
        Ensure.NotEmpty(personId);
        Ensure.HasValue(countryCode);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        
        Address instance = new(personId, countryCode, name, region, city, maybeLine.HasValue ? maybeLine.Value : null);
        return Result.Success(instance);
    }

    private Address(
        PersonId personId,
        CountryCode countryCode,
        AddressName name,
        AddressRegion region,
        AddressCity city,
        AddressLine? line)
    {
        PersonId = personId;
        CountryCode = countryCode;
        Name = name;
        Region = region;
        City = city;
        Line = line;
    }
}
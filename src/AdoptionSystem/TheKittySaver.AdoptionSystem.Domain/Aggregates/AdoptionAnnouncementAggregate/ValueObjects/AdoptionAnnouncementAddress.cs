using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds.Specifications;
using TheKittySaver.AdoptionSystem.Primitives.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementAddress : ValueObject
{
    public CountryCode CountryCode { get; }
    public AddressPostalCode PostalCode { get; }
    public AddressRegion Region { get; }
    public AddressCity City { get; }
    public AddressLine? Line { get; }

    public static Result<AdoptionAnnouncementAddress> Create(
        IAddressConsistencySpecification specification,
        CountryCode countryCode,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        Maybe<AddressLine> line)
    {
        ArgumentNullException.ThrowIfNull(specification);
        ArgumentNullException.ThrowIfNull(postalCode);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(line);

        if (!specification.IsSatisfiedBy(countryCode, postalCode.Value, region.Value, out Error? error))
        {
            return Result.Failure<AdoptionAnnouncementAddress>(error!);
        }

        AdoptionAnnouncementAddress instance = new(
            countryCode,
            postalCode,
            region,
            city,
            line.HasValue
                ? line.Value
                : null);
        return Result.Success(instance);
    }

    private AdoptionAnnouncementAddress(
        CountryCode countryCode,
        AddressPostalCode postalCode,
        AddressRegion region,
        AddressCity city,
        AddressLine? line)
    {
        CountryCode = countryCode;
        PostalCode = postalCode;
        Region = region;
        City = city;
        Line = line;
    }

    private AdoptionAnnouncementAddress()
    {
        PostalCode = null!;
        Region = null!;
        City = null!;
    }

    public override string ToString()
    {
        string lineText = Line is not null ? $"{Line.Value}, " : string.Empty;
        return $"{lineText}{City}, {Region}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CountryCode;
        yield return PostalCode;
        yield return Region;
        yield return City;

        if (Line is not null)
        {
            yield return Line.Value;
        }
    }
}

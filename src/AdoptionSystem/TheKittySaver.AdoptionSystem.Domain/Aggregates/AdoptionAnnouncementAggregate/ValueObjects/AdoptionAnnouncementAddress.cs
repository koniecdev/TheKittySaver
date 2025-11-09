using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementAddress : ValueObject
{
    public AddressCity City { get; }
    public AddressRegion Region { get; }
    public AddressLine? Line { get; }

    public static Result<AdoptionAnnouncementAddress> Create(
        AddressCity city,
        AddressRegion region,
        Maybe<AddressLine> line)
    {
        ArgumentNullException.ThrowIfNull(city);
        ArgumentNullException.ThrowIfNull(region);
        ArgumentNullException.ThrowIfNull(line);

        AdoptionAnnouncementAddress instance = new(
            city,
            region,
            line.HasValue 
                ? line.Value 
                : null);
        return Result.Success(instance);
    }

    private AdoptionAnnouncementAddress(
        AddressCity city,
        AddressRegion region,
        AddressLine? line)
    {
        City = city;
        Region = region;
        Line = line;
    }

    public override string ToString()
    {
        string lineText = Line is not null ? $"{Line.Value}, " : string.Empty;
        return $"{lineText}{City}, {Region}";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return City;
        yield return Region;

        if (Line is not null)
        {
            yield return Line.Value;
        }
    }
}

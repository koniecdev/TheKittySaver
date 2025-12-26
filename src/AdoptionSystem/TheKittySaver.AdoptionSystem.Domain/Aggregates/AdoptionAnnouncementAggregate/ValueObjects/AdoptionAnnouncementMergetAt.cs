using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AdoptionAnnouncementMergetAt : ValueObject
{
    public static readonly DateTimeOffset MinimumAllowedValue =
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset Value { get; }

    public static Result<AdoptionAnnouncementMergetAt> Create(DateTimeOffset value)
    {
        Ensure.NotEmpty(value);

        if (value < MinimumAllowedValue)
        {
            return Result.Failure<AdoptionAnnouncementMergetAt>(DomainErrors.PublishedAtValueObject.CannotBeInThePast);
        }

        AdoptionAnnouncementMergetAt instance = new(value);
        return Result.Success(instance);
    }

    private AdoptionAnnouncementMergetAt(DateTimeOffset value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("O");

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

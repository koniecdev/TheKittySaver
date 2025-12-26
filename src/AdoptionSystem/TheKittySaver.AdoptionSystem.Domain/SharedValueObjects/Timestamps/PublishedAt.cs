using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

public sealed class PublishedAt : ValueObject
{
    public static readonly DateTimeOffset MinimumAllowedValue =
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset Value { get; }

    public static Result<PublishedAt> Create(DateTimeOffset value)
    {
        Ensure.NotEmpty(value);

        if (value < MinimumAllowedValue)
        {
            return Result.Failure<PublishedAt>(DomainErrors.PublishedAtValueObject.CannotBeInThePast);
        }

        PublishedAt instance = new(value);
        return Result.Success(instance);
    }

    private PublishedAt(DateTimeOffset value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("O");

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

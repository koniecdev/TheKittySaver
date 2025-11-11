using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

public sealed class ArchivedAt : ValueObject
{
    public static readonly DateTimeOffset MinimumAllowedValue = 
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset Value { get; }

    public static Result<ArchivedAt> Create(DateTimeOffset value)
    {
        if (value < MinimumAllowedValue)
        {
            return Result.Failure<ArchivedAt>(DomainErrors.ArchivedAtValueObject.CannotBeInThePast);
        }

        ArchivedAt instance = new(value);
        return Result.Success(instance);
    }

    private ArchivedAt(DateTimeOffset value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("O");

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

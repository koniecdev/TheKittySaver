using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

public sealed class CreatedAt : ValueObject
{
    public static readonly DateTimeOffset MinimumAllowedValue = 
        new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public DateTimeOffset Value { get; }

    public static Result<CreatedAt> Create(DateTimeOffset value)
    {
        if (value < MinimumAllowedValue)
        {
            return Result.Failure<CreatedAt>(DomainErrors.CreatedAtValueObject.CannotBeInThePast);
        }

        CreatedAt instance = new(value);
        return Result.Success(instance);
    }

    private CreatedAt(DateTimeOffset value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("O");

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

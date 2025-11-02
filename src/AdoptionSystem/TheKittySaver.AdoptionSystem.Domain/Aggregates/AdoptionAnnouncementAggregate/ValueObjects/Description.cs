using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class Description : ValueObject
{
    public const int MaxLength = 150;
    public string Value { get; }

    public static Result<Description> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Description>();
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<Description>();
            
        }

        value = value.Trim();

        Description instance = new(value);
        return Result.Success(instance);
    }

    private Description(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(Description value) => value.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
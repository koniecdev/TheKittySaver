using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionPriorityScore : ValueObject
{
    public const decimal MinimumAllowedValue = decimal.Zero;
    public const decimal MaximumAllowedValue = 170;
    public decimal Value { get; }
    
    public static AdoptionPriorityScore Create(decimal value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(value, MinimumAllowedValue);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(value, MaximumAllowedValue);
        AdoptionPriorityScore instance = new(value);
        return instance;
    }

    private AdoptionPriorityScore(decimal value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
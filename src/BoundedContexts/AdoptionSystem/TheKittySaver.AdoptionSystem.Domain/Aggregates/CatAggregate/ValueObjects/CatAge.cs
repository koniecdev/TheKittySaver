using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatAge : ValueObject
{
    public const int MinimumAllowedValue = 0;
    public const int MaximumAllowedValue = 38;
    public int Value { get; }

    public override string ToString() => Value.ToString();
    public static implicit operator int(CatAge value) => value.Value;

    public static Result<CatAge> Create(int value)
    {
        Result<CatAge> result = Result.Success(value)
            .Ensure(v => v >= MinimumAllowedValue , 
                DomainErrors.CatEntity.CatAgeProperty.BelowMinimalAllowedValue(value, MinimumAllowedValue))
            .Ensure(v => v <= MaximumAllowedValue, 
                DomainErrors.CatEntity.CatAgeProperty.AboveMaximumAllowedValue(value, MaximumAllowedValue))
            .Map(v => new CatAge(v));
        return result;
    }

    private CatAge(int value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
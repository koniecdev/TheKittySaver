using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatAge : ValueObject
{
    public const int MinimumAllowedValue = 0;
    public const int MaximumAllowedValue = 38;
    public int Value { get; }
    
    public static Result<CatAge> Create(int value)
    {
        switch (value)
        {
            case < MinimumAllowedValue:
                return Result.Failure<CatAge>(
                    DomainErrors.CatEntity.CatAgeProperty
                        .BelowMinimalAllowedValue(value, MinimumAllowedValue));
            case > MaximumAllowedValue:
                return Result.Failure<CatAge>(
                    DomainErrors.CatEntity.CatAgeProperty
                        .AboveMaximumAllowedValue(value, MaximumAllowedValue));
            default:
            {
                CatAge instance = new(value);
                return Result.Success(instance);
            }
        }
    }

    private CatAge(int value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
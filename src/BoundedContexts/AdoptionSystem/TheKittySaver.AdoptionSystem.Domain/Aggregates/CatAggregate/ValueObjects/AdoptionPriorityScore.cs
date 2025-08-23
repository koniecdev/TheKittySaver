using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionPriorityScore : ValueObject
{
    public const decimal MinimumAllowedValue = decimal.Zero;
    public const decimal MaximumAllowedValue = 170;
    public decimal Value { get; }
    
    public static Result<AdoptionPriorityScore> Create(decimal value)
    {
        switch (value)
        {
            case < MinimumAllowedValue:
                return Result.Failure<AdoptionPriorityScore>(
                    DomainErrors.AdoptionPriorityScoreValueObject
                        .BelowMinimalAllowedValue(value, MinimumAllowedValue));
            case > MaximumAllowedValue:
                return Result.Failure<AdoptionPriorityScore>(
                    DomainErrors.AdoptionPriorityScoreValueObject
                        .AboveMaximumAllowedValue(value, MaximumAllowedValue));
            default:
            {
                AdoptionPriorityScore instance = new(value);
                return Result.Success(instance);
            }
        }
    }

    private AdoptionPriorityScore(decimal value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
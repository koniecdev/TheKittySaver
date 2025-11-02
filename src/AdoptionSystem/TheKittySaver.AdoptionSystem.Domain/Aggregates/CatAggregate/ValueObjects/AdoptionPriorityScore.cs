using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class AdoptionPriorityScore : ValueObject
{
    public const decimal MinimumAllowedPoints = decimal.Zero;
    public const decimal MaximumAllowedPoints = 170;
    public decimal Value { get; }
    
    public static Result<AdoptionPriorityScore> Create(decimal value)
    {
        switch (value)
        {
            case < MinimumAllowedPoints:
                return Result.Failure<AdoptionPriorityScore>(
                    DomainErrors.AdoptionPriorityScoreValueObject
                        .BelowMinimalAllowedValue(value, MinimumAllowedPoints));
            case > MaximumAllowedPoints:
                return Result.Failure<AdoptionPriorityScore>(
                    DomainErrors.AdoptionPriorityScoreValueObject
                        .AboveMaximumAllowedValue(value, MaximumAllowedPoints));
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
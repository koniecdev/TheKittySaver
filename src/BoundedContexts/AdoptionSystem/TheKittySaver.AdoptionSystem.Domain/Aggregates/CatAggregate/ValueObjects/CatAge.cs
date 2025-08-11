using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatAge : ValueObject
{
    public const int MinimumAllowedValue = 0;
    public const int MaximumAllowedValue = 38;
    public int Value { get; }
    
    public AdoptionPriorityScore CalculatePriorityScore()
    {
        decimal points = Value switch
        {
            >= 10 => 30,
            >= 7 => 25,
            >= 5 => 20,
            >= 3 => 15,
            >= 1 => 10,
            _ => 5
        };
        
        Result<AdoptionPriorityScore> result = AdoptionPriorityScore.Create(points);
        
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException("Something went wrong while calculating priority points");
    }
    
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

    public override string ToString() => Value.ToString();
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
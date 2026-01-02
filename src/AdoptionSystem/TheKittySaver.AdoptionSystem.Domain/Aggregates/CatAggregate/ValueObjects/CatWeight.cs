using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatWeight : ValueObject
{
    public const int MinWeightGrams = 100;
    public const int MaxWeightGrams = 20000;

    public int ValueInGrams { get; }

    public static Result<CatWeight> Create(int valueInGrams)
    {
        switch (valueInGrams)
        {
            case < MinWeightGrams:
                return Result.Failure<CatWeight>(
                    DomainErrors.CatEntity.WeightProperty.BelowMinimum(valueInGrams, MinWeightGrams));
            case > MaxWeightGrams:
                return Result.Failure<CatWeight>(
                    DomainErrors.CatEntity.WeightProperty.AboveMaximum(valueInGrams, MaxWeightGrams));
            default:
                {
                    CatWeight instance = new(valueInGrams);
                    return Result.Success(instance);
                }
        }
    }

    private CatWeight(int valueInGrams)
    {
        ValueInGrams = valueInGrams;
    }

    public override string ToString() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.###} kg", ValueInGrams / 1000.0m);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ValueInGrams;
    }
}

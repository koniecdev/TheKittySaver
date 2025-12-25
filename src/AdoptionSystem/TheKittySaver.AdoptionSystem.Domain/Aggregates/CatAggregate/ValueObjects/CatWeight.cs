using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatWeight : ValueObject
{
    public const decimal MinWeightKg = 0.1m;
    public const decimal MaxWeightKg = 20m;
    
    public decimal ValueInKilograms { get; }
    
    public static Result<CatWeight> Create(decimal valueInKilograms)
    {
        switch (valueInKilograms)
        {
            case < MinWeightKg:
                return Result.Failure<CatWeight>(
                    DomainErrors.CatEntity.WeightProperty.BelowMinimum(valueInKilograms, MinWeightKg));
            case > MaxWeightKg:
                return Result.Failure<CatWeight>(
                    DomainErrors.CatEntity.WeightProperty.AboveMaximum(valueInKilograms, MaxWeightKg));
            default:
            {
                CatWeight instance = new(valueInKilograms);
                return Result.Success(instance);
            }
        }
    }

    private CatWeight(decimal valueInKilograms)
    {
        ValueInKilograms = valueInKilograms;
    }

    public override string ToString() => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0} kg", ValueInKilograms);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return ValueInKilograms;
    }
}

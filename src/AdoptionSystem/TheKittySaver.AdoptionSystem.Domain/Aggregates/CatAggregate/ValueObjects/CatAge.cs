using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Consts;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatAge : ValueObject
{
    public const int MinimumAllowedValue = 0;
    public const int MaximumAllowedValue = 40;
    public int Value { get; }
    
    public static bool IsDateTooOldForCat(DateTimeOffset date, DateTimeOffset currentDate)
    {
        Ensure.NotEmpty(date);
        Ensure.NotEmpty(currentDate);

        DateTimeOffset oldestAllowedDate = currentDate.Subtract(TimeSpan.FromDays(UtilityConsts.AverageOfDaysInOneYear * MaximumAllowedValue));
        return date < oldestAllowedDate;
    }

    public static bool IsDateTooOldForCat(DateOnly date, DateOnly currentDate)
    {
        DateOnly oldestAllowedDate = currentDate.AddDays((int)(-UtilityConsts.AverageOfDaysInOneYear * MaximumAllowedValue));
        return date < oldestAllowedDate;
    }

    public static Result<CatAge> Create(int value)
    {
        switch (value)
        {
            case < MinimumAllowedValue:
                return Result.Failure<CatAge>(
                    DomainErrors.CatEntity.AgeProperty
                        .BelowMinimalAllowedValue(value, MinimumAllowedValue));
            case > MaximumAllowedValue:
                return Result.Failure<CatAge>(
                    DomainErrors.CatEntity.AgeProperty
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

    public override string ToString() => Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

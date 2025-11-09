using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatAge : ValueObject
{
    public const int MinimumAllowedValue = 0;
    public const int MaximumAllowedValue = 40;
    public const double AverageOfDaysInOneYear = 365.2425;
    public int Value { get; }
    
    public static bool IsDateTooOldForCat(DateTimeOffset date, DateTimeOffset currentDate)
    {
        DateTimeOffset oldestAllowedDate = currentDate.Subtract(TimeSpan.FromDays(AverageOfDaysInOneYear * MaximumAllowedValue));
        return date < oldestAllowedDate;
    }

    public static bool IsDateTooOldForCat(DateOnly date, DateOnly currentDate)
    {
        DateOnly oldestAllowedDate = currentDate.AddDays((int)(-AverageOfDaysInOneYear * MaximumAllowedValue));
        return date < oldestAllowedDate;
    }

    public static Result<CatAge> Create(int value)
    {
        switch (value)
        {
            case < MinimumAllowedValue:
                return Result.Failure<CatAge>(
                    DomainErrors.CatEntity.AgeValueObject
                        .BelowMinimalAllowedValue(value, MinimumAllowedValue));
            case > MaximumAllowedValue:
                return Result.Failure<CatAge>(
                    DomainErrors.CatEntity.AgeValueObject
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
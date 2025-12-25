using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class VaccinationDate : ValueObject
{
    public DateOnly Value { get; }

    public static Result<VaccinationDate> Create(
        DateOnly value,
        DateOnly referenceDate)
    {
        if (value > referenceDate)
        {
            return Result.Failure<VaccinationDate>(
                DomainErrors.VaccinationEntity.DateProperty.VaccinationDateInFuture(value, referenceDate));
        }

        if (CatAge.IsDateTooOldForCat(value, referenceDate))
        {
            return Result.Failure<VaccinationDate>(
                DomainErrors.VaccinationEntity.DateProperty.VaccinationDateTooOld(value, referenceDate));
        }

        VaccinationDate instance = new(value);
        return Result.Success(instance);
    }

    private VaccinationDate(DateOnly value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}

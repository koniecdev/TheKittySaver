using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class VaccinationDates : ValueObject
{
    public DateTimeOffset VaccinationDate { get; }
    public DateTimeOffset? NextDueDate { get; }

    public static Result<VaccinationDates> Create(
        DateTimeOffset vaccinationDate,
        DateTimeOffset referenceDate,
        DateTimeOffset? nextDueDate = null)
    {
        if (vaccinationDate > referenceDate)
        {
            return Result.Failure<VaccinationDates>(
                DomainErrors.CatVaccinationEntity.VaccinationDatesValueObject.VaccinationDateInFuture(vaccinationDate, referenceDate));
        }

        if (CatAge.IsDateTooOldForCat(vaccinationDate, referenceDate))
        {
            return Result.Failure<VaccinationDates>(
                DomainErrors.CatVaccinationEntity.VaccinationDatesValueObject.VaccinationDateTooOld(vaccinationDate, referenceDate));
        }

        if (nextDueDate.HasValue && nextDueDate.Value < referenceDate)
        {
            return Result.Failure<VaccinationDates>(
                DomainErrors.CatVaccinationEntity.VaccinationDatesValueObject.NextDueDateInPast(nextDueDate.Value, referenceDate));
        }

        if (nextDueDate.HasValue && nextDueDate.Value <= vaccinationDate)
        {
            return Result.Failure<VaccinationDates>(
                DomainErrors.CatVaccinationEntity.VaccinationDatesValueObject.NextDueDateBeforeOrEqualVaccinationDate(nextDueDate.Value, vaccinationDate));
        }

        VaccinationDates instance = new(vaccinationDate, nextDueDate);
        return Result.Success(instance);
    }

    private VaccinationDates(DateTimeOffset vaccinationDate, DateTimeOffset? nextDueDate)
    {
        VaccinationDate = vaccinationDate;
        NextDueDate = nextDueDate;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return VaccinationDate;
        if (NextDueDate.HasValue)
        {
            yield return NextDueDate.Value;
        }
    }
}

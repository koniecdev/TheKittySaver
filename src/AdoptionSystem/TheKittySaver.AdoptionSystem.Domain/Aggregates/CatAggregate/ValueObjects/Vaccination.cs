using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class Vaccination : ValueObject
{
    public VaccinationType Type { get; }
    public DateTimeOffset VaccinationDate { get; }
    public DateTimeOffset? NextDueDate { get; }
    public string? VeterinarianNote { get; }
    
    public const int MaxVeterinarianNoteLength = 200;
    
    public static Result<Vaccination> Create(
        VaccinationType type,
        DateTimeOffset vaccinationDate,
        DateTimeOffset currentDate,
        DateTimeOffset? nextDueDate = null,
        string? veterinarianNote = null)
    {
        if (vaccinationDate > currentDate)
        {
            return Result.Failure<Vaccination>(
                DomainErrors.CatEntity.VaccinationValueObject.DateInFuture);
        }

        const double averageOfDaysInOneYear = 365.2425;
        if (vaccinationDate < currentDate.Subtract(TimeSpan.FromDays(averageOfDaysInOneYear * CatAge.MaximumAllowedValue)))
        {
            return Result.Failure<Vaccination>(
                DomainErrors.CatEntity.VaccinationValueObject.DateTooOld);
        }

        if (nextDueDate.HasValue && nextDueDate.Value < currentDate)
        {
            return Result.Failure<Vaccination>(
                DomainErrors.CatEntity.VaccinationValueObject.NextDueDateInPast);
        }

        if (veterinarianNote is not null)
        {
            veterinarianNote = veterinarianNote.Trim();
            if (veterinarianNote.Length > MaxVeterinarianNoteLength)
            {
                return Result.Failure<Vaccination>(
                    DomainErrors.CatEntity.VaccinationValueObject.NoteTooLong);
            }
        }

        Vaccination instance = new(type, vaccinationDate, nextDueDate, veterinarianNote);
        return Result.Success(instance);
    }

    private Vaccination(
        VaccinationType type, 
        DateTimeOffset vaccinationDate, 
        DateTimeOffset? nextDueDate,
        string? veterinarianNote)
    {
        Type = type;
        VaccinationDate = vaccinationDate;
        NextDueDate = nextDueDate;
        VeterinarianNote = veterinarianNote;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Type;
        yield return VaccinationDate;
        if (NextDueDate is not null)
        {
            yield return NextDueDate;
        }
        if (VeterinarianNote is not null)
        {
            yield return VeterinarianNote;
        }
    }
}
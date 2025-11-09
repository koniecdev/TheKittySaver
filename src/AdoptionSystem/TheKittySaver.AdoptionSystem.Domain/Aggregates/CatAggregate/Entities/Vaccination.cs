using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Vaccination : Entity<VaccinationId>
{
    public VaccinationType Type { get; private set; }
    public DateTimeOffset VaccinationDate { get; private set; }
    public DateTimeOffset? NextDueDate { get; private set; }
    public VaccinationNote? VeterinarianNote { get; private set; }
    
    public Result UpdateType(VaccinationType updatedType)
    {
        Type = updatedType;
        return Result.Success();
    }

    public Result UpdateVaccinationDate(DateTimeOffset updatedVaccinationDate, DateTimeOffset currentDate)
    {
        if (updatedVaccinationDate > currentDate)
        {
            return Result.Failure(DomainErrors.CatVaccinationEntity.DateInFuture);
        }

        if (CatAge.IsDateTooOldForCat(updatedVaccinationDate, currentDate))
        {
            return Result.Failure(DomainErrors.CatVaccinationEntity.DateTooOld);
        }

        VaccinationDate = updatedVaccinationDate;
        return Result.Success();
    }

    public Result UpdateNextDueDate(DateTimeOffset? updatedNextDueDate, DateTimeOffset currentDate)
    {
        if (updatedNextDueDate.HasValue && updatedNextDueDate.Value < currentDate)
        {
            return Result.Failure(DomainErrors.CatVaccinationEntity.NextDueDateInPast);
        }

        NextDueDate = updatedNextDueDate;
        return Result.Success();
    }

    public Result UpdateVeterinarianNote(VaccinationNote? updatedVeterinarianNote)
    {
        VeterinarianNote = updatedVeterinarianNote;
        return Result.Success();
    }
    
    public static Result<Vaccination> Create(
        VaccinationType type,
        DateTimeOffset vaccinationDate,
        DateTimeOffset currentDate,
        DateTimeOffset? nextDueDate = null,
        VaccinationNote? veterinarianNote = null)
    {
        if (vaccinationDate > currentDate)
        {
            return Result.Failure<Vaccination>(
                DomainErrors.CatVaccinationEntity.DateInFuture);
        }

        if (CatAge.IsDateTooOldForCat(vaccinationDate, currentDate))
        {
            return Result.Failure<Vaccination>(
                DomainErrors.CatVaccinationEntity.DateTooOld);
        }

        if (nextDueDate.HasValue && nextDueDate.Value < currentDate)
        {
            return Result.Failure<Vaccination>(
                DomainErrors.CatVaccinationEntity.NextDueDateInPast);
        }

        VaccinationId id = VaccinationId.New();
        Vaccination instance = new(id, type, vaccinationDate, nextDueDate, veterinarianNote);
        return Result.Success(instance);
    }

    private Vaccination(
        VaccinationId id,
        VaccinationType type,
        DateTimeOffset vaccinationDate,
        DateTimeOffset? nextDueDate,
        VaccinationNote? veterinarianNote) : base(id)
    {
        Type = type;
        VaccinationDate = vaccinationDate;
        NextDueDate = nextDueDate;
        VeterinarianNote = veterinarianNote;
    }
}
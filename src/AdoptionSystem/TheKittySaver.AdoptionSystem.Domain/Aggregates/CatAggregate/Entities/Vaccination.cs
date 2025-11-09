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
    public VaccinationDates Dates { get; private set; }
    public VaccinationNote? VeterinarianNote { get; private set; }
    
    public Result UpdateType(VaccinationType updatedType)
    {
        Type = updatedType;
        return Result.Success();
    }

    public Result UpdateVaccinationDate(DateTimeOffset updatedVaccinationDate, DateTimeOffset currentDate)
    {
        Result<VaccinationDates> newDatesResult = VaccinationDates.Create(
            updatedVaccinationDate,
            currentDate,
            Dates.NextDueDate);

        if (!newDatesResult.IsSuccess)
        {
            return Result.Failure(newDatesResult.Error);
        }

        Dates = newDatesResult.Value;
        return Result.Success();
    }

    public Result UpdateNextDueDate(DateTimeOffset? updatedNextDueDate, DateTimeOffset currentDate)
    {
        Result<VaccinationDates> newDatesResult = VaccinationDates.Create(
            Dates.VaccinationDate,
            currentDate,
            updatedNextDueDate);

        if (!newDatesResult.IsSuccess)
        {
            return Result.Failure(newDatesResult.Error);
        }

        Dates = newDatesResult.Value;
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
        CreatedAt createdAt,
        DateTimeOffset? nextDueDate = null,
        VaccinationNote? veterinarianNote = null)
    {
        ArgumentNullException.ThrowIfNull(createdAt);

        Result<VaccinationDates> datesResult = VaccinationDates.Create(
            vaccinationDate,
            createdAt.Value,
            nextDueDate);

        if (!datesResult.IsSuccess)
        {
            return Result.Failure<Vaccination>(datesResult.Error);
        }

        VaccinationId id = VaccinationId.New();
        Vaccination instance = new(id, type, datesResult.Value, veterinarianNote, createdAt);
        return Result.Success(instance);
    }

    private Vaccination(
        VaccinationId id,
        VaccinationType type,
        VaccinationDates dates,
        VaccinationNote? veterinarianNote,
        CreatedAt createdAt) : base(id, createdAt)
    {
        Type = type;
        Dates = dates;
        VeterinarianNote = veterinarianNote;
    }
}
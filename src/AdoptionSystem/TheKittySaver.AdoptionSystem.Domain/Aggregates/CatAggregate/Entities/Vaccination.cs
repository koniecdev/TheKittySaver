using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Vaccination : Entity<VaccinationId>
{
    public VaccinationType Type { get; private set; }
    public VaccinationDate Date { get; private set; }
    public VaccinationNote? VeterinarianNote { get; private set; }
    
    internal Result UpdateType(VaccinationType updatedType)
    {
        Type = updatedType;
        return Result.Success();
    }

    internal Result UpdateDate(VaccinationDate updatedDate)
    {
        ArgumentNullException.ThrowIfNull(updatedDate);

        Date = updatedDate;
        return Result.Success();
    }

    internal Result UpdateVeterinarianNote(VaccinationNote? updatedVeterinarianNote)
    {
        VeterinarianNote = updatedVeterinarianNote;
        return Result.Success();
    }
    
    internal static Result<Vaccination> Create(
        VaccinationType type,
        DateOnly vaccinationDate,
        CreatedAt createdAt,
        VaccinationNote? veterinarianNote = null)
    {
        ArgumentNullException.ThrowIfNull(createdAt);

        Result<VaccinationDate> dateResult = VaccinationDate.Create(
            vaccinationDate,
            DateOnly.FromDateTime(createdAt.Value.DateTime));

        if (!dateResult.IsSuccess)
        {
            return Result.Failure<Vaccination>(dateResult.Error);
        }

        VaccinationId id = VaccinationId.New();
        Vaccination instance = new(id, type, dateResult.Value, veterinarianNote, createdAt);
        return Result.Success(instance);
    }

    private Vaccination(
        VaccinationId id,
        VaccinationType type,
        VaccinationDate date,
        VaccinationNote? veterinarianNote,
        CreatedAt createdAt) : base(id, createdAt)
    {
        Type = type;
        Date = date;
        VeterinarianNote = veterinarianNote;
    }
}

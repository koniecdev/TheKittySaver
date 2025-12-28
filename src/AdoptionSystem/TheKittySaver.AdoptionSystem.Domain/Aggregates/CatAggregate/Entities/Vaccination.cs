using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Vaccination : Entity<VaccinationId>
{
    public CatId CatId { get; }
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
        CatId catId,
        VaccinationType type,
        DateOnly vaccinationDate,
        DateTimeOffset dateTimeOfOperation,
        VaccinationNote? veterinarianNote = null)
    {
        Ensure.NotEmpty(catId);
        Ensure.IsValidEnum(type);

        Result<VaccinationDate> dateResult = VaccinationDate.Create(
            vaccinationDate,
            DateOnly.FromDateTime(dateTimeOfOperation.UtcDateTime));

        if (!dateResult.IsSuccess)
        {
            return Result.Failure<Vaccination>(dateResult.Error);
        }

        VaccinationId id = VaccinationId.Create();
        Vaccination instance = new(catId, id, type, dateResult.Value, veterinarianNote);
        return Result.Success(instance);
    }

    private Vaccination(
        CatId catId,
        VaccinationId id,
        VaccinationType type,
        VaccinationDate date,
        VaccinationNote? veterinarianNote) : base(id)
    {
        CatId = catId;
        Type = type;
        Date = date;
        VeterinarianNote = veterinarianNote;
    }

    private Vaccination()
    {
        Date = null!;
    }
}

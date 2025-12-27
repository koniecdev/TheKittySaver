using System.Diagnostics.CodeAnalysis;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class Vaccination : Entity<VaccinationId>, IArchivable
{
    public CatId CatId { get; }
    public VaccinationType Type { get; private set; }
    public VaccinationDate Date { get; private set; }
    public VaccinationNote? VeterinarianNote { get; private set; }
    public ArchivedAt? ArchivedAt { get; private set; }

    public Result Archive(ArchivedAt archivedAt)
    {
        ArgumentNullException.ThrowIfNull(archivedAt);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        ArchivedAt = archivedAt;
        return Result.Success();
    }

    public Result Unarchive()
    {
        if (ArchivedAt is null)
        {
            return Result.Failure(DomainErrors.VaccinationEntity.IsNotArchived(Id));
        }

        ArchivedAt = null;
        return Result.Success();
    }

    internal Result UpdateType(VaccinationType updatedType)
    {
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        Type = updatedType;
        return Result.Success();
    }

    internal Result UpdateDate(VaccinationDate updatedDate)
    {
        ArgumentNullException.ThrowIfNull(updatedDate);
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

        Date = updatedDate;
        return Result.Success();
    }

    internal Result UpdateVeterinarianNote(VaccinationNote? updatedVeterinarianNote)
    {
        if (IsArchived(out Result? failure))
        {
            return failure;
        }

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
        Ensure.IsInEnum(type);

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

    private bool IsArchived([NotNullWhen(true)] out Result? failure)
    {
        bool isArchived = ArchivedAt is not null;

        failure = isArchived
            ? Result.Failure(DomainErrors.VaccinationEntity.IsArchived(Id))
            : Result.Success();

        return isArchived;
    }
}

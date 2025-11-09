using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatStatus : ValueObject
{
    public const int MaxStatusNoteLength = 300;
    
    public CatStatusType Value { get; }
    public DateTimeOffset StatusChangedAt { get; }
    public string? StatusNote { get; }
    
    public bool IsDraft => Value is CatStatusType.Draft;
    public bool IsPublished => Value is CatStatusType.Published;
    public bool IsAdopted => Value is CatStatusType.Adopted;

    public static Result<CatStatus> Draft(DateTimeOffset changedAt)
        => CreateWithoutNote(CatStatusType.Draft, changedAt);

    public static Result<CatStatus> Published(DateTimeOffset changedAt)
        => CreateWithoutNote(CatStatusType.Published, changedAt);

    public static Result<CatStatus> Adopted(DateTimeOffset changedAt, string? note = null)
        => CreateWithNote(CatStatusType.Adopted, changedAt, note);
    
    private static Result<CatStatus> CreateWithoutNote(
        CatStatusType statusType, 
        DateTimeOffset changedAt)
    {
        CatStatus instance = new(statusType, changedAt, null);
        return Result.Success(instance);
    }
    
    private static Result<CatStatus> CreateWithNote(
        CatStatusType statusType, 
        DateTimeOffset changedAt, 
        string? note)
    {
        if (note is not null)
        {
            note = note.Trim();
            if (note.Length > MaxStatusNoteLength)
            {
                return Result.Failure<CatStatus>(
                    DomainErrors.CatEntity.StatusValueObject.NoteTooManyCharacters);
            }
        }
        
        CatStatus instance = new(statusType, changedAt, note);
        return Result.Success(instance);
    }
    
    private CatStatus(CatStatusType value, DateTimeOffset statusChangedAt, string? statusNote)
    {
        Value = value;
        StatusChangedAt = statusChangedAt;
        StatusNote = statusNote;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
        yield return StatusChangedAt;
        if (StatusNote is not null)
        {
            yield return StatusNote;
        }
    }
}
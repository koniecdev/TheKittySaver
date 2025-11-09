using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;

public sealed class AnnouncementStatus : ValueObject
{
    public const int MaxStatusNoteLength = 300;
    
    public AnnouncementStatusType Value { get; }
    public DateTimeOffset StatusChangedAt { get; }
    public string? StatusNote { get; }
    
    public bool IsActive => Value is AnnouncementStatusType.Active;
    public bool IsArchived => Value is AnnouncementStatusType.Archived;

    public static Result<AnnouncementStatus> Active(DateTimeOffset changedAt)
        => CreateWithoutNote(AnnouncementStatusType.Active, changedAt);

    public static Result<AnnouncementStatus> Archived(DateTimeOffset changedAt, string? note = null)
        => CreateWithNote(AnnouncementStatusType.Archived, changedAt, note);
    
    private static Result<AnnouncementStatus> CreateWithoutNote(
        AnnouncementStatusType statusType, 
        DateTimeOffset changedAt)
    {
        AnnouncementStatus instance = new(statusType, changedAt, null);
        return Result.Success(instance);
    }
    
    private static Result<AnnouncementStatus> CreateWithNote(
        AnnouncementStatusType statusType, 
        DateTimeOffset changedAt, 
        string? note)
    {
        if (note is not null)
        {
            note = note.Trim();
            if (note.Length > MaxStatusNoteLength)
            {
                return Result.Failure<AnnouncementStatus>(
                    DomainErrors.AdoptionAnnouncementEntity.StatusValueObject.NoteTooLong);
            }
        }
        
        AnnouncementStatus instance = new(statusType, changedAt, note);
        return Result.Success(instance);
    }
    
    private AnnouncementStatus(
        AnnouncementStatusType value, 
        DateTimeOffset statusChangedAt, 
        string? statusNote)
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
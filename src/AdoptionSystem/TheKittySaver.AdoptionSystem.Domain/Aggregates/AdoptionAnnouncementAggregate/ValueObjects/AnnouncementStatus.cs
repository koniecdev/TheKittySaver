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
    
    public bool IsDraft => Value is AnnouncementStatusType.Draft;
    public bool IsActive => Value is AnnouncementStatusType.Active;
    public bool IsPaused => Value is AnnouncementStatusType.Paused;
    public bool IsCompleted => Value is AnnouncementStatusType.Completed;
    public bool IsCancelled => Value is AnnouncementStatusType.Cancelled;
    
    public static Result<AnnouncementStatus> Draft(DateTimeOffset changedAt) 
        => CreateWithoutNote(AnnouncementStatusType.Draft, changedAt);
    
    public static Result<AnnouncementStatus> Active(DateTimeOffset changedAt) 
        => CreateWithoutNote(AnnouncementStatusType.Active, changedAt);
    
    public static Result<AnnouncementStatus> Paused(DateTimeOffset changedAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<AnnouncementStatus>(
                DomainErrors.AdoptionAnnouncementEntity.StatusValueObject.PauseReasonRequired);
        }
        
        return CreateWithNote(AnnouncementStatusType.Paused, changedAt, reason);
    }
    
    public static Result<AnnouncementStatus> Completed(DateTimeOffset changedAt, string? note = null)
        => CreateWithNote(AnnouncementStatusType.Completed, changedAt, note);
    
    public static Result<AnnouncementStatus> Cancelled(DateTimeOffset changedAt, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            return Result.Failure<AnnouncementStatus>(
                DomainErrors.AdoptionAnnouncementEntity.StatusValueObject.CancelReasonRequired);
        }
        
        return CreateWithNote(AnnouncementStatusType.Cancelled, changedAt, reason);
    }
    
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
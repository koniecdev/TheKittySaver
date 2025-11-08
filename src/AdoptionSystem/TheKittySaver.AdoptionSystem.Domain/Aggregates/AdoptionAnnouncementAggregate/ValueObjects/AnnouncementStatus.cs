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
    
    
    public static AnnouncementStatus Draft(DateTimeOffset changedAt) 
        => new(AnnouncementStatusType.Draft, changedAt, null);
    
    public static AnnouncementStatus Active(DateTimeOffset changedAt) 
        => new(AnnouncementStatusType.Active, changedAt, null);
    
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
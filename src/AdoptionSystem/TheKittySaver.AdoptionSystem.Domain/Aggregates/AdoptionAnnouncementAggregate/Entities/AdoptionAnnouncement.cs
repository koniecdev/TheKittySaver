using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;

public sealed class AdoptionAnnouncement : AggregateRoot<AdoptionAnnouncementId>
{
    public PersonId PersonId { get; }
    public AdoptionAnnouncementDescription? Description { get; private set; }
    public AdoptionAnnouncementAddress Address { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public AnnouncementStatus Status { get; private set; }
    
    public Result UpdateDescription(Maybe<AdoptionAnnouncementDescription> updatedDescription)
    {
        ArgumentNullException.ThrowIfNull(updatedDescription);
        Description = updatedDescription.HasValue 
            ? updatedDescription.Value 
            : null;
        return Result.Success();
    }

    public Result UpdateAddress(AdoptionAnnouncementAddress updatedAddress)
    {
        ArgumentNullException.ThrowIfNull(updatedAddress);

        if (Status.Value is not (AnnouncementStatusType.Draft or AnnouncementStatusType.Active or AnnouncementStatusType.Paused))
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyUpdateAddressWhenDraftActiveOrPaused);
        }

        Address = updatedAddress;
        return Result.Success();
    }

    public Result UpdateEmail(Email updatedEmail)
    {
        ArgumentNullException.ThrowIfNull(updatedEmail);

        if (Status.Value is not (AnnouncementStatusType.Draft or AnnouncementStatusType.Active or AnnouncementStatusType.Paused))
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyUpdateEmailWhenDraftActiveOrPaused);
        }

        Email = updatedEmail;
        return Result.Success();
    }

    public Result UpdatePhoneNumber(PhoneNumber updatedPhoneNumber)
    {
        ArgumentNullException.ThrowIfNull(updatedPhoneNumber);

        if (Status.Value is not (AnnouncementStatusType.Draft or AnnouncementStatusType.Active or AnnouncementStatusType.Paused))
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyUpdatePhoneNumberWhenDraftActiveOrPaused);
        }

        PhoneNumber = updatedPhoneNumber;
        return Result.Success();
    }

    public Result Publish(DateTimeOffset publishedAt)
    {
        if (Status.Value != AnnouncementStatusType.Draft)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyPublishDraft);
        }

        Status = AnnouncementStatus.Active(publishedAt);
        return Result.Success();
    }
    
    public Result Pause(DateTimeOffset pausedAt, string reason)
    {
        if (Status.Value != AnnouncementStatusType.Active)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyPauseActive);
        }

        Result<AnnouncementStatus> pauseResult = AnnouncementStatus.Paused(pausedAt, reason);
        if (pauseResult.IsFailure)
        {
            return Result.Failure(pauseResult.Error);
        }

        Status = pauseResult.Value;
        return Result.Success();
    }
    
    public Result Resume(DateTimeOffset resumedAt)
    {
        if (Status.Value != AnnouncementStatusType.Paused)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyResumeWhenPaused);
        }

        Status = AnnouncementStatus.Active(resumedAt);
        return Result.Success();
    }
    
    public Result Complete(DateTimeOffset completedAt, string? note = null)
    {
        if (Status.Value is not (AnnouncementStatusType.Active or AnnouncementStatusType.Paused))
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyCompleteActiveOrPaused);
        }

        Result<AnnouncementStatus> completeResult = AnnouncementStatus.Completed(completedAt, note);
        if (completeResult.IsFailure)
        {
            return Result.Failure(completeResult.Error);
        }

        Status = completeResult.Value;
        return Result.Success();
    }
    
    public Result Cancel(DateTimeOffset cancelledAt, string reason)
    {
        if (Status.Value is AnnouncementStatusType.Completed or AnnouncementStatusType.Cancelled)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CannotCancelFinishedAnnouncement);
        }

        Result<AnnouncementStatus> cancelResult = AnnouncementStatus.Cancelled(cancelledAt, reason);
        if (cancelResult.IsFailure)
        {
            return Result.Failure(cancelResult.Error);
        }

        Status = cancelResult.Value;
        return Result.Success();
    }
    
    internal static Result<AdoptionAnnouncement> Create(
        PersonId personId,
        Maybe<AdoptionAnnouncementDescription> description,
        AdoptionAnnouncementAddress address,
        Email email,
        PhoneNumber phoneNumber,
        CreatedAt createdAt)
    {
        Ensure.NotEmpty(personId);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(phoneNumber);
        ArgumentNullException.ThrowIfNull(createdAt);

        AdoptionAnnouncementId id = AdoptionAnnouncementId.New();
        AdoptionAnnouncement instance = new(
            id,
            personId,
            description,
            address,
            email,
            phoneNumber,
            createdAt);

        return Result.Success(instance);
    }

    private AdoptionAnnouncement(
        AdoptionAnnouncementId id,
        PersonId personId,
        AdoptionAnnouncementDescription? description,
        AdoptionAnnouncementAddress address,
        Email email,
        PhoneNumber phoneNumber,
        CreatedAt createdAt) : base(id, createdAt)
    {
        PersonId = personId;
        Description = description;
        Address = address;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = AnnouncementStatus.Draft(createdAt.Value);
    }
}
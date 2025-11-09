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

        if (!Status.IsActive)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyUpdateWhenActive);
        }

        Address = updatedAddress;
        return Result.Success();
    }

    public Result UpdateEmail(Email updatedEmail)
    {
        ArgumentNullException.ThrowIfNull(updatedEmail);

        if (!Status.IsActive)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyUpdateWhenActive);
        }

        Email = updatedEmail;
        return Result.Success();
    }

    public Result UpdatePhoneNumber(PhoneNumber updatedPhoneNumber)
    {
        ArgumentNullException.ThrowIfNull(updatedPhoneNumber);

        if (!Status.IsActive)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.CanOnlyUpdateWhenActive);
        }

        PhoneNumber = updatedPhoneNumber;
        return Result.Success();
    }

    public Result Archive(DateTimeOffset archivedAt, string? note = null)
    {
        if (Status.IsArchived)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.AlreadyArchived);
        }

        Result<AnnouncementStatus> archiveResult = AnnouncementStatus.Archived(archivedAt, note);
        if (archiveResult.IsFailure)
        {
            return Result.Failure(archiveResult.Error);
        }

        Status = archiveResult.Value;
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

        var statusResult = AnnouncementStatus.Active(createdAt.Value);
        if (statusResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncement>(statusResult.Error);
        }
        
        AdoptionAnnouncementId id = AdoptionAnnouncementId.New();
        AdoptionAnnouncement instance = new(
            id,
            personId,
            description,
            address,
            email,
            phoneNumber,
            statusResult.Value,
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
        AnnouncementStatus status,
        CreatedAt createdAt) : base(id, createdAt)
    {
        PersonId = personId;
        Description = description;
        Address = address;
        Email = email;
        PhoneNumber = phoneNumber;
        Status = status;
    }
}
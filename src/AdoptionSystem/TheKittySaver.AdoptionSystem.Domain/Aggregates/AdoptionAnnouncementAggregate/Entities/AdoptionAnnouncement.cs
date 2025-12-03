using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;

public sealed class AdoptionAnnouncement : AggregateRoot<AdoptionAnnouncementId>, IClaimable
{
    private readonly List<AdoptionAnnouncementMergeLog> _mergeLogs = [];
    public PersonId PersonId { get; }
    public ClaimedAt? ClaimedAt { get; private set; }
    public AdoptionAnnouncementDescription? Description { get; private set; }
    public AdoptionAnnouncementAddress Address { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber PhoneNumber { get; private set; }
    public AnnouncementStatusType Status { get; private set; }

    public IReadOnlyList<AdoptionAnnouncementMergeLog> MergeLogs => _mergeLogs.AsReadOnly();

    public Result PersistAdoptionAnnouncementAfterLastCatReassignment(AdoptionAnnouncementId deletedAdoptionAnnouncementId)
    {
        AdoptionAnnouncementMergeLog log = AdoptionAnnouncementMergeLog.Create(deletedAdoptionAnnouncementId);
        if (_mergeLogs.Contains(log))
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.MergeLogsProperty.AlreadyExists);
        }
        _mergeLogs.Add(log);
        return Result.Success();
    }
    
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

        if (Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.StatusProperty.CanOnlyUpdateWhenActive);
        }

        Address = updatedAddress;
        return Result.Success();
    }

    public Result UpdateEmail(Email updatedEmail)
    {
        ArgumentNullException.ThrowIfNull(updatedEmail);

        if (Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.StatusProperty.CanOnlyUpdateWhenActive);
        }

        Email = updatedEmail;
        return Result.Success();
    }

    public Result UpdatePhoneNumber(PhoneNumber updatedPhoneNumber)
    {
        ArgumentNullException.ThrowIfNull(updatedPhoneNumber);

        if (Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementErrors.StatusProperty.CanOnlyUpdateWhenActive);
        }

        PhoneNumber = updatedPhoneNumber;
        return Result.Success();
    }
    
    public Result Claim(ClaimedAt claimedAt)
    {
        ArgumentNullException.ThrowIfNull(claimedAt);
        if (Status is AnnouncementStatusType.Claimed)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementErrors.StatusProperty.AlreadyClaimed(Id));
        }

        Status = AnnouncementStatusType.Claimed;
        ClaimedAt = claimedAt;

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
            description.HasValue ? description.Value : null,
            address,
            email,
            phoneNumber,
            AnnouncementStatusType.Active,
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
        AnnouncementStatusType status,
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
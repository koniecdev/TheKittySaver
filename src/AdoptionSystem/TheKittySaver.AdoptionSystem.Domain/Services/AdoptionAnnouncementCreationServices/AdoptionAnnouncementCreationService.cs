using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Services.AdoptionAnnouncementCreationServices;

public sealed class AdoptionAnnouncementCreationService : IAdoptionAnnouncementCreationService
{
    private readonly ICatAdoptionAnnouncementAssignmentService _assignmentService;

    public AdoptionAnnouncementCreationService(
        ICatAdoptionAnnouncementAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }

    public Result<AdoptionAnnouncement> Create(
        IReadOnlyCollection<Cat> catsToAssign,
        AdoptionAnnouncementAddress address,
        Email email,
        PhoneNumber phoneNumber,
        Maybe<AdoptionAnnouncementDescription> description,
        DateTimeOffset dateTimeOfOperation)
    {
        if (catsToAssign.Count == 0)
        {
            return Result.Failure<AdoptionAnnouncement>(DomainErrors.AdoptionAnnouncementEntity.NoCatsProvided);
        }

        PersonId personId = catsToAssign.First().PersonId;

        Result<AdoptionAnnouncement> aaCreationResult = AdoptionAnnouncement.Create(
            personId: personId,
            description: description,
            address: address,
            email: email,
            phoneNumber: phoneNumber);

        if (aaCreationResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncement>(aaCreationResult.Error);
        }

        AdoptionAnnouncement announcement = aaCreationResult.Value;
        List<Cat> assignedCats = [];

        foreach (Cat cat in catsToAssign)
        {
            Result assignmentResult = _assignmentService.AssignCatToAdoptionAnnouncement(
                cat,
                announcement,
                assignedCats,
                dateTimeOfOperation);

            if (assignmentResult.IsFailure)
            {
                return Result.Failure<AdoptionAnnouncement>(assignmentResult.Error);
            }

            assignedCats.Add(cat);
        }

        return Result.Success(announcement);
    }
}

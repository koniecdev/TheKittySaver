using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

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
        Cat catToAssign,
        AdoptionAnnouncementAddress address,
        Email email,
        PhoneNumber phoneNumber,
        Maybe<AdoptionAnnouncementDescription> description,
        DateTimeOffset dateTimeOfOperation,
        CreatedAt createdAt)
    {
        Result<AdoptionAnnouncement> aaCreationResult = AdoptionAnnouncement.Create(
            personId: catToAssign.PersonId,
            description: description,
            address: address,
            email: email,
            phoneNumber: phoneNumber,
            createdAt: createdAt);
            
        if (aaCreationResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncement>(aaCreationResult.Error);
        }
        
        Result assignmentResult = _assignmentService.AssignCatToAdoptionAnnouncement(
            catToAssign,
            aaCreationResult.Value,
            [],
            dateTimeOfOperation);
            
        return assignmentResult.IsFailure 
            ? Result.Failure<AdoptionAnnouncement>(assignmentResult.Error)
            : Result.Success(aaCreationResult.Value);
    }
}
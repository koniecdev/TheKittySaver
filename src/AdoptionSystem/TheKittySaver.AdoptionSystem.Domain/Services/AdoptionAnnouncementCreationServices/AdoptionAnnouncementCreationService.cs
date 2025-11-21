using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.PhoneNumbers;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Services.AdoptionAnnouncementCreationServices;

public sealed class AdoptionAnnouncementCreationService : IAdoptionAnnouncementCreationService
{
    private readonly ICatAdoptionAnnouncementAssignmentService _catAdoptionAnnouncementAssignmentService;

    public AdoptionAnnouncementCreationService(
        ICatAdoptionAnnouncementAssignmentService catAdoptionAnnouncementAssignmentService)
    {
        _catAdoptionAnnouncementAssignmentService = catAdoptionAnnouncementAssignmentService;
    }
    
    public async Task<Result<AdoptionAnnouncement>> CreateAsync(
        Cat catToAssign,
        AdoptionAnnouncementAddress address,
        Email email,
        PhoneNumber phoneNumber,
        Maybe<AdoptionAnnouncementDescription> description,
        CreatedAt createdAt,
        CancellationToken cancellationToken)
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
        
        Result catToAdoptionAnnouncementAssignmentResult = await _catAdoptionAnnouncementAssignmentService
            .AssignCatToAdoptionAnnouncementAsync(catToAssign, aaCreationResult.Value, cancellationToken);
        if (catToAdoptionAnnouncementAssignmentResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncement>(catToAdoptionAnnouncementAssignmentResult.Error);
        }
        
        return Result.Success(aaCreationResult.Value);
    } 
}
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
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
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.AdoptionAnnouncementCreationServices;

public sealed class AdoptionAnnouncementCreationService
{
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
    private readonly ICatRepository _catRepository;
    private readonly ICatAdoptionAnnouncementAssignmentService _catAdoptionAnnouncementAssignmentService;

    public AdoptionAnnouncementCreationService(
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
        ICatRepository catRepository,
        ICatAdoptionAnnouncementAssignmentService catAdoptionAnnouncementAssignmentService)
    {
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
        _catRepository = catRepository;
        _catAdoptionAnnouncementAssignmentService = catAdoptionAnnouncementAssignmentService;
    }
    
    public async Task<Result<AdoptionAnnouncementId>> CreateAndInsertAsync(
        CatId catToAssignId,
        AdoptionAnnouncementAddress address,
        Email email,
        PhoneNumber phoneNumber,
        Maybe<AdoptionAnnouncementDescription> description,
        CreatedAt createdAt,
        CancellationToken cancellationToken)
    {
        Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(catToAssignId, cancellationToken);
        if (maybeCat.HasNoValue)
        {
            return Result.Failure<AdoptionAnnouncementId>(
                DomainErrors.CatEntity.NotFound(catToAssignId));
        }
        
        Result<AdoptionAnnouncement> aaCreationResult = AdoptionAnnouncement.Create(
            personId: maybeCat.Value.PersonId,
            description: description,
            address: address,
            email: email,
            phoneNumber: phoneNumber,
            createdAt: createdAt);
        if (aaCreationResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncementId>(aaCreationResult.Error);
        }
        
        Result catToAdoptionAnnouncementAssignmentResult = await _catAdoptionAnnouncementAssignmentService
            .AssignCatToAdoptionAnnouncementAsync(maybeCat.Value, aaCreationResult.Value, cancellationToken);
        if (catToAdoptionAnnouncementAssignmentResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncementId>(catToAdoptionAnnouncementAssignmentResult.Error);
        }
        
        _adoptionAnnouncementRepository.Insert(aaCreationResult.Value);
        return Result.Success(aaCreationResult.Value.Id);
    } 
}
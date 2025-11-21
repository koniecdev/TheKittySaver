using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementUnassignmentServices;

internal sealed class CatAdoptionAnnouncementUnassignmentService : ICatAdoptionAnnouncementUnassignmentService
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;

    public CatAdoptionAnnouncementUnassignmentService(
        ICatRepository catRepository,
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository)
    {
        _catRepository = catRepository;
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
    }

    public async Task<Result> UnassignCatFromAdoptionAnnouncementAsync(
        Cat cat,
        AdoptionAnnouncement adoptionAnnouncement,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<Cat> catsInitiallyAssignedToAdoptionAnnouncement = 
            await _catRepository.GetCatsByAdoptionAnnouncementIdAsync(adoptionAnnouncement.Id, cancellationToken);
        
        if (adoptionAnnouncement.Status is AnnouncementStatusType.Claimed)
        {
            return Result.Failure(
                DomainErrors.AdoptionAnnouncementEntity.AdoptionAnnouncementAlreadyClaimed(adoptionAnnouncement.Id));
        }

        if (cat.AdoptionAnnouncementId != adoptionAnnouncement.Id)
        {
            return Result.Failure(
                DomainErrors.CatAdoptionAnnouncementService.CatNotAssignedToAdoptionAnnouncement(cat.Id, adoptionAnnouncement.Id));
        }

        Result unassignmentResult = cat.UnassignFromAdoptionAnnouncement();
        if(unassignmentResult.IsFailure)
        {
            return Result.Failure(unassignmentResult.Error);
        }
        
        bool aaStillContainsAtLeastOneCat =
            catsInitiallyAssignedToAdoptionAnnouncement.Any(assignedCat => assignedCat != cat);

        if (!aaStillContainsAtLeastOneCat)
        {
            _adoptionAnnouncementRepository.Remove(adoptionAnnouncement);
        }
        
        return Result.Success();
    }
}
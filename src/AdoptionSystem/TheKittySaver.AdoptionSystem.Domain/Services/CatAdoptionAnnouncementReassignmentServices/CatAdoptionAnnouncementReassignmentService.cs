using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementReassignmentServices;

internal sealed class CatAdoptionAnnouncementReassignmentService : ICatAdoptionAnnouncementReassignmentService
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;

    public CatAdoptionAnnouncementReassignmentService(
        ICatRepository catRepository,
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository)
    {
        _catRepository = catRepository;
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
    }

    public async Task<Result> ReassignCatToAnotherAdoptionAnnouncementAsync(
        Cat cat,
        AdoptionAnnouncement sourceAdoptionAnnouncement,
        AdoptionAnnouncement destinationAdoptionAnnouncement,
        DateTimeOffset dateTimeOfOperation,
        CancellationToken cancellationToken = default)
    {
        if (sourceAdoptionAnnouncement.Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.CannotReassignCatToInactiveAdoptionAnnouncement);
        }

        if (destinationAdoptionAnnouncement.Status is not AnnouncementStatusType.Active)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.CannotReassignCatToInactiveAdoptionAnnouncement);
        }
        
        IReadOnlyCollection<Cat> catsInitiallyAssignedToSourceAdoptionAnnouncement = await _catRepository
            .GetCatsByAdoptionAnnouncementIdAsync(sourceAdoptionAnnouncement.Id, cancellationToken);
        IReadOnlyCollection<Cat> catsInitiallyAssignedToDestinationAdoptionAnnouncement = await _catRepository
            .GetCatsByAdoptionAnnouncementIdAsync(destinationAdoptionAnnouncement.Id, cancellationToken);

        if (catsInitiallyAssignedToDestinationAdoptionAnnouncement.Contains(cat))
        {
            return Result.Failure(DomainErrors.CatEntity.CannotReassignCatToSameAdoptionAnnouncement);
        }
        
        var isCatCompatibileWithOthers =
            catsInitiallyAssignedToDestinationAdoptionAnnouncement.All(alreadyAssignedCat =>
                alreadyAssignedCat.InfectiousDiseaseStatus.IsCompatibleWith(cat.InfectiousDiseaseStatus));

        if (!isCatCompatibileWithOthers)
        {
            return Result.Failure(DomainErrors.CatEntity.CannotReassignCatToIncompatibleAdoptionAnnouncement);
        }

        var reassignmentResult = cat.ReassignToAnotherAdoptionAnnouncement(
            destinationAdoptionAnnouncement.Id,
            dateTimeOfOperation);
        if (reassignmentResult.IsFailure)
        {
            return reassignmentResult;
        }
        
        bool sourceAaStillContainsAtLeastOneCat =
            catsInitiallyAssignedToSourceAdoptionAnnouncement.Any(assignedCat => assignedCat != cat);
        if (!sourceAaStillContainsAtLeastOneCat)
        {
            _adoptionAnnouncementRepository.Remove(sourceAdoptionAnnouncement);
        }
        
        return Result.Success();
    }
}
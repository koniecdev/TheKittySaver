using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatClaimServices;

internal sealed class CatClaimService
{
    private readonly ICatRepository _catRepository;

    public CatClaimService(ICatRepository catRepository)
    {
        _catRepository = catRepository;
    }
    
    public async Task<Result> ClaimAdoptionAnnouncementCatsAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        AnnouncementStatusType adoptionAnnouncementStatus,
        ClaimedAt? adoptionAnnouncementClaimedAt,
        CancellationToken cancellationToken = default)
    {
        if (adoptionAnnouncementStatus is not AnnouncementStatusType.Claimed
            || adoptionAnnouncementClaimedAt is null)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.IsNotClaimed);
        }
        
        IReadOnlyCollection<Cat> aaCats = await _catRepository
            .GetCatsByAdoptionAnnouncementIdAsync(adoptionAnnouncementId, cancellationToken);

        foreach (Cat cat in aaCats)
        {
            Result catClaimResult = cat.Claim(adoptionAnnouncementClaimedAt);
            if (catClaimResult.IsFailure)
            {
                return catClaimResult;
            }
        }
        
        return Result.Success();
    }
}
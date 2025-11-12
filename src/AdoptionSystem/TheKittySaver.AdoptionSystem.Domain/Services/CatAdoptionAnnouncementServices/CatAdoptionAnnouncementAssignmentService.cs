using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;

internal sealed class CatAdoptionAnnouncementAssignmentService
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;

    public CatAdoptionAnnouncementAssignmentService(
        ICatRepository catRepository,
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository)
    {
        _catRepository = catRepository;
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
    }
    
    public async Task<Result> ReassignCatToAdoptionAnnouncementAsync(
        CatId catId,
        AdoptionAnnouncementId adoptionAnnouncementId,
        DateTimeOffset currentDate,
        CancellationToken cancellationToken = default)
    {
        Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(catId, cancellationToken);
        if (maybeCat.HasNoValue)
        {
            return Result.Failure(DomainErrors.CatEntity.NotFound(catId));
        }

        if (maybeCat.Value.AdoptionAnnouncementId.HasValue
            && maybeCat.Value.AdoptionAnnouncementId == adoptionAnnouncementId)
        {
            return Result.Success();
        }

        Maybe<AdoptionAnnouncement> maybeAdoptionAnnouncement =
            await _adoptionAnnouncementRepository.GetByIdAsync(adoptionAnnouncementId, cancellationToken);
        if (maybeAdoptionAnnouncement.HasNoValue)
        {
            return Result.Failure(DomainErrors.AdoptionAnnouncementEntity.NotFound(adoptionAnnouncementId));
        }

        if (maybeCat.Value.PersonId != maybeAdoptionAnnouncement.Value.PersonId)
        {
            return Result.Failure(DomainErrors.CatAdoptionAnnouncementService.PersonIdMismatch(
                catId,
                maybeCat.Value.PersonId,
                adoptionAnnouncementId,
                maybeAdoptionAnnouncement.Value.PersonId));
        }

        if (maybeCat.Value.AdoptionAnnouncementId.HasValue)
        {
            Maybe<AdoptionAnnouncement> maybeOldAnnouncement =
                await _adoptionAnnouncementRepository.GetByIdAsync(
                    maybeCat.Value.AdoptionAnnouncementId.Value,
                    cancellationToken);

            if (maybeOldAnnouncement.HasValue)
            {
                Result<ArchivedAt> archivedAtResult = ArchivedAt.Create(currentDate);
                if (archivedAtResult.IsFailure)
                {
                    return archivedAtResult;
                }
                
                Result archiveResult = maybeOldAnnouncement.Value.Archive(archivedAtResult.Value);
                if (archiveResult.IsFailure)
                {
                    return archiveResult;
                }
            }
        }

        var reassignResult = maybeCat.Value.ReassignToAdoptionAnnouncement(maybeAdoptionAnnouncement.Value.Id);
        return reassignResult;
    }

    public async Task<Result<AdoptionAnnouncement?>> UnassignCatFromAnnouncementAsync(
        CatId catId,
        DateTimeOffset currentDate,
        CancellationToken cancellationToken = default)
    {
        Maybe<Cat> maybeCat = await _catRepository.GetByIdAsync(catId, cancellationToken);
        if (maybeCat.HasNoValue)
        {
            return Result.Failure<AdoptionAnnouncement?>(DomainErrors.CatEntity.NotFound(catId));
        }

        if (!maybeCat.Value.AdoptionAnnouncementId.HasValue)
        {
            return Result.Success<AdoptionAnnouncement?>(null); // Already unassigned
        }

        var unassignResult = maybeCat.Value.UnassignFromAdoptionAnnouncement();
        if (unassignResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncement?>(unassignResult.Error);
        }

        // If cat is Published, create a new announcement for it automatically
        // This happens when user removes cat from a family but wants it to remain published
        if (maybeCat.Value.Status is CatStatusType.Published)
        {
            // TODO: This will need to be implemented by event handler
            // For now, we return null and the event handler will create the announcement
            return Result.Success<AdoptionAnnouncement?>(null);
        }

        return Result.Success<AdoptionAnnouncement?>(null);
    }
}
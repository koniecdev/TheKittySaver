using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Services.CatAdoptionAnnouncementServices;

internal sealed class CatAdoptionAnnouncementService
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;

    public CatAdoptionAnnouncementService(
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
                Result archiveResult = maybeOldAnnouncement.Value.Archive(
                    currentDate,
                    "Cat moved to another announcement");

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
            return Result.Success<AdoptionAnnouncement?>(null);
        }

        var unassignResult = maybeCat.Value.UnassignFromAdoptionAnnouncement();
        if (unassignResult.IsFailure)
        {
            return Result.Failure<AdoptionAnnouncement?>(unassignResult.Error);
        }

        return Result.Success<AdoptionAnnouncement?>(null);
    }
}
using Mediator;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;

namespace TheKittySaver.AdoptionSystem.API.DomainEventHandlers;

internal sealed class CatClaimedDomainEventHandler
    : INotificationHandler<CatClaimedDomainEvent>
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CatClaimedDomainEventHandler> _logger;

    public CatClaimedDomainEventHandler(
        ICatRepository catRepository,
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<CatClaimedDomainEventHandler> logger)
    {
        _catRepository = catRepository;
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async ValueTask Handle(
        CatClaimedDomainEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Cat {CatId} was claimed from announcement {AnnouncementId}",
            notification.CatId.Value,
            notification.AdoptionAnnouncementId.Value);

        int unclaimedCatsCount = await _catRepository.CountUnclaimedCatsByAdoptionAnnouncementIdAsync(
            notification.AdoptionAnnouncementId,
            cancellationToken);

        if (unclaimedCatsCount == 0)
        {
            _logger.LogInformation(
                "All cats in announcement {AnnouncementId} have been claimed, claiming the announcement",
                notification.AdoptionAnnouncementId.Value);

            Maybe<AdoptionAnnouncement> maybeAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                notification.AdoptionAnnouncementId,
                cancellationToken);

            if (maybeAnnouncement.HasValue)
            {
                Result<ClaimedAt> claimedAtResult = ClaimedAt.Create(_timeProvider.GetUtcNow());
                if (claimedAtResult.IsSuccess)
                {
                    Result claimResult = maybeAnnouncement.Value.Claim(claimedAtResult.Value);
                    if (claimResult.IsSuccess)
                    {
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
            }
        }
    }
}

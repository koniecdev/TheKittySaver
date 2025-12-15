using Mediator;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;

namespace TheKittySaver.AdoptionSystem.API.DomainEventHandlers;

internal sealed class CatUnassignedFromAnnouncementDomainEventHandler
    : INotificationHandler<CatUnassignedFromAnnouncementDomainEvent>
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CatUnassignedFromAnnouncementDomainEventHandler> _logger;

    public CatUnassignedFromAnnouncementDomainEventHandler(
        ICatRepository catRepository,
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
        IUnitOfWork unitOfWork,
        ILogger<CatUnassignedFromAnnouncementDomainEventHandler> logger)
    {
        _catRepository = catRepository;
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask Handle(
        CatUnassignedFromAnnouncementDomainEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Cat {CatId} was unassigned from announcement {AnnouncementId}",
            notification.CatId.Value,
            notification.AdoptionAnnouncementId.Value);

        int remainingCatsCount = await _catRepository.CountCatsByAdoptionAnnouncementIdAsync(
            notification.AdoptionAnnouncementId,
            cancellationToken);

        if (remainingCatsCount == 0)
        {
            _logger.LogInformation(
                "Announcement {AnnouncementId} has no more cats, removing it",
                notification.AdoptionAnnouncementId.Value);

            Maybe<AdoptionAnnouncement> maybeAnnouncement = await _adoptionAnnouncementRepository.GetByIdAsync(
                notification.AdoptionAnnouncementId,
                cancellationToken);

            if (maybeAnnouncement.HasValue)
            {
                _adoptionAnnouncementRepository.Remove(maybeAnnouncement.Value);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

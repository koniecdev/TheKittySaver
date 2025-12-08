using Mediator;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;

namespace TheKittySaver.AdoptionSystem.API.DomainEventHandlers;

internal sealed class CatReassignedToAnotherAnnouncementDomainEventHandler
    : INotificationHandler<CatReassignedToAnotherAnnouncementNotification>
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CatReassignedToAnotherAnnouncementDomainEventHandler> _logger;

    public CatReassignedToAnotherAnnouncementDomainEventHandler(
        ICatRepository catRepository,
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
        IUnitOfWork unitOfWork,
        ILogger<CatReassignedToAnotherAnnouncementDomainEventHandler> logger)
    {
        _catRepository = catRepository;
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async ValueTask Handle(
        CatReassignedToAnotherAnnouncementNotification notification,
        CancellationToken cancellationToken)
    {
        CatReassignedToAnotherAnnouncementDomainEvent domainEvent = notification.DomainEvent;

        _logger.LogInformation(
            "Cat {CatId} was reassigned from announcement {SourceAnnouncementId} to {DestinationAnnouncementId}",
            domainEvent.CatId.Value,
            domainEvent.SourceAdoptionAnnouncementId.Value,
            domainEvent.DestinationAdoptionAnnouncementId.Value);

        int remainingCatsInSource = await _catRepository.CountCatsByAdoptionAnnouncementIdAsync(
            domainEvent.SourceAdoptionAnnouncementId,
            cancellationToken);

        if (remainingCatsInSource == 0)
        {
            _logger.LogInformation(
                "Source announcement {SourceAnnouncementId} has no more cats, will be deleted and merge logged in {DestinationAnnouncementId}",
                domainEvent.SourceAdoptionAnnouncementId.Value,
                domainEvent.DestinationAdoptionAnnouncementId.Value);

            Maybe<AdoptionAnnouncement> maybeDestination = await _adoptionAnnouncementRepository.GetByIdAsync(
                domainEvent.DestinationAdoptionAnnouncementId,
                cancellationToken);

            if (maybeDestination.HasValue)
            {
                maybeDestination.Value.PersistAdoptionAnnouncementAfterLastCatReassignment(
                    domainEvent.SourceAdoptionAnnouncementId);
            }

            Maybe<AdoptionAnnouncement> maybeSource = await _adoptionAnnouncementRepository.GetByIdAsync(
                domainEvent.SourceAdoptionAnnouncementId,
                cancellationToken);

            if (maybeSource.HasValue)
            {
                _adoptionAnnouncementRepository.Remove(maybeSource.Value);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

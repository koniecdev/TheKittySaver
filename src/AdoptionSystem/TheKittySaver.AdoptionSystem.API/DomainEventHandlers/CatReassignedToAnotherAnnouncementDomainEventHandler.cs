using Mediator;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.Abstractions;

namespace TheKittySaver.AdoptionSystem.API.DomainEventHandlers;

internal sealed class CatReassignedToAnotherAnnouncementDomainEventHandler
    : INotificationHandler<CatReassignedToAnotherAnnouncementDomainEvent>
{
    private readonly ICatRepository _catRepository;
    private readonly IAdoptionAnnouncementRepository _adoptionAnnouncementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<CatReassignedToAnotherAnnouncementDomainEventHandler> _logger;

    public CatReassignedToAnotherAnnouncementDomainEventHandler(
        ICatRepository catRepository,
        IAdoptionAnnouncementRepository adoptionAnnouncementRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider,
        ILogger<CatReassignedToAnotherAnnouncementDomainEventHandler> logger)
    {
        _catRepository = catRepository;
        _adoptionAnnouncementRepository = adoptionAnnouncementRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    public async ValueTask Handle(
        CatReassignedToAnotherAnnouncementDomainEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Cat {CatId} was reassigned from announcement {SourceAnnouncementId} to {DestinationAnnouncementId}",
            notification.CatId.Value,
            notification.SourceAdoptionAnnouncementId.Value,
            notification.DestinationAdoptionAnnouncementId.Value);

        int remainingCatsInSource = await _catRepository.CountCatsByAdoptionAnnouncementIdAsync(
            notification.SourceAdoptionAnnouncementId,
            cancellationToken);

        if (remainingCatsInSource == 0)
        {
            _logger.LogInformation(
                "Source announcement {SourceAnnouncementId} has no more cats, will be deleted and merge logged in {DestinationAnnouncementId}",
                notification.SourceAdoptionAnnouncementId.Value,
                notification.DestinationAdoptionAnnouncementId.Value);

            Maybe<AdoptionAnnouncement> maybeDestination = await _adoptionAnnouncementRepository.GetByIdAsync(
                notification.DestinationAdoptionAnnouncementId,
                cancellationToken);

            if (maybeDestination.HasValue)
            {
                Result<AdoptionAnnouncementMergetAt> mergedAtResult = AdoptionAnnouncementMergetAt.Create(
                    _timeProvider.GetUtcNow());

                if (mergedAtResult.IsFailure)
                {
                    throw new InvalidOperationException("Failed to create merged at");
                }

                Result persistResult = maybeDestination.Value.PersistAdoptionAnnouncementAfterLastCatReassignment(
                    notification.SourceAdoptionAnnouncementId,
                    mergedAtResult.Value);
                if (persistResult.IsFailure)
                {
                    throw new InvalidOperationException("Failed to persist merged at");
                }
            }

            Maybe<AdoptionAnnouncement> maybeSource = await _adoptionAnnouncementRepository.GetByIdAsync(
                notification.SourceAdoptionAnnouncementId,
                cancellationToken);

            if (maybeSource.HasValue)
            {
                _adoptionAnnouncementRepository.Remove(maybeSource.Value);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

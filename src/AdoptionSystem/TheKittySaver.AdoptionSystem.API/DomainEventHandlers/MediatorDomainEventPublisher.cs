using Mediator;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Events;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.API.DomainEventHandlers;

internal sealed class MediatorDomainEventPublisher : IDomainEventPublisher
{
    private readonly IPublisher _publisher;

    public MediatorDomainEventPublisher(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            INotification? notification = CreateNotification(domainEvent);
            if (notification is not null)
            {
                await _publisher.Publish(notification, cancellationToken);
            }
        }
    }

    private static INotification? CreateNotification(IDomainEvent domainEvent)
    {
        return domainEvent switch
        {
            CatClaimedDomainEvent e => new CatClaimedNotification(e),
            CatReassignedToAnotherAnnouncementDomainEvent e => new CatReassignedToAnotherAnnouncementNotification(e),
            CatUnassignedFromAnnouncementDomainEvent e => new CatUnassignedFromAnnouncementNotification(e),
            _ => null
        };
    }
}

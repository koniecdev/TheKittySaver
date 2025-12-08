using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.API.DomainEventHandlers;

internal interface IDomainEventPublisher
{
    Task PublishDomainEventsAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}

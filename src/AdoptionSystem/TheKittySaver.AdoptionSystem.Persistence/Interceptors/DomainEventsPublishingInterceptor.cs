using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Persistence.Interceptors;

internal sealed class DomainEventsPublishingInterceptor : SaveChangesInterceptor
{
    private readonly IPublisher _publisher;

    public DomainEventsPublishingInterceptor(IPublisher publisher)
    {
        _publisher = publisher;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await PublishDomainEventsAsync(eventData.Context, cancellationToken);
        }

        return result;
    }

    private async Task PublishDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        List<IAggregateRoot> aggregatesWithEvents = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(entry => entry.Entity.GetDomainEvents().Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        List<IDomainEvent> domainEvents = aggregatesWithEvents
            .SelectMany(aggregate => aggregate.GetDomainEvents())
            .ToList();

        foreach (IAggregateRoot aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await _publisher.Publish(domainEvent, cancellationToken);
        }
    }
}

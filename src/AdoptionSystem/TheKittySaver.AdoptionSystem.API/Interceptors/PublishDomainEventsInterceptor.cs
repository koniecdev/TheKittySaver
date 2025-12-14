using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TheKittySaver.AdoptionSystem.API.DomainEventHandlers;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.API.Interceptors;

internal sealed class PublishDomainEventsInterceptor : SaveChangesInterceptor
{
    private readonly IDomainEventPublisher _domainEventPublisher;

    public PublishDomainEventsInterceptor(IDomainEventPublisher domainEventPublisher)
    {
        _domainEventPublisher = domainEventPublisher;
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

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private async Task PublishDomainEventsAsync(DbContext context, CancellationToken cancellationToken)
    {
        List<IAggregateRoot> aggregateRoots = [.. context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.GetDomainEvents().Count != 0)
            .Select(e => e.Entity)];

        List<IDomainEvent> domainEvents = [.. aggregateRoots.SelectMany(a => a.GetDomainEvents())];

        foreach (IAggregateRoot aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        await _domainEventPublisher.PublishDomainEventsAsync(domainEvents, cancellationToken);
    }
}

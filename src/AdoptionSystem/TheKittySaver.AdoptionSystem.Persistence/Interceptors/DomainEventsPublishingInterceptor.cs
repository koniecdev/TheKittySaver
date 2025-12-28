using Mediator;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Persistence.Interceptors;

internal sealed class DomainEventsPublishingInterceptor : SaveChangesInterceptor
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public DomainEventsPublishingInterceptor(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return result;
        }

        List<IAggregateRoot> aggregatesWithEvents = eventData.Context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(entry => entry.Entity.DomainEvents.Count > 0)
            .Select(entry => entry.Entity)
            .ToList();

        List<IDomainEvent> domainEvents = aggregatesWithEvents
            .SelectMany(aggregate => aggregate.DomainEvents)
            .ToList();

        foreach (IAggregateRoot aggregate in aggregatesWithEvents)
        {
            aggregate.ClearDomainEvents();
        }

        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        IPublisher publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await publisher.Publish(domainEvent, cancellationToken);
        }

        return result;
    }
}

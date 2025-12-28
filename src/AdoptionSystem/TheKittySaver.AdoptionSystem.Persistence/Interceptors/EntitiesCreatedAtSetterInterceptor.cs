using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.EntityFramework.Aggregates;

namespace TheKittySaver.AdoptionSystem.Persistence.Interceptors;

internal sealed class EntitiesCreatedAtSetterInterceptor : SaveChangesInterceptor
{
    private readonly TimeProvider _timeProvider;
    public EntitiesCreatedAtSetterInterceptor(
        TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
        {
            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        List<EntityEntry<IEntity>> addedEntitiesWithCreatedAtPropEntries = eventData.Context.ChangeTracker
            .Entries<IEntity>()
            .Where(entry => entry.State is EntityState.Added
                            && entry.Metadata.FindProperty(EntityConfiguration.CreatedAt) is not null)
            .ToList();

        foreach (EntityEntry<IEntity> entry in addedEntitiesWithCreatedAtPropEntries)
        {
            entry.Property(EntityConfiguration.CreatedAt).CurrentValue = _timeProvider.GetUtcNow();
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}

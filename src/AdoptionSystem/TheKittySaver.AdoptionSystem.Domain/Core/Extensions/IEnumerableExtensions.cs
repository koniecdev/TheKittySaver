using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Extensions;

public static class IEnumerableExtensions
{
    public static Maybe<TEntity> GetByIdOrDefault<TEntity, TEntityId>(
        this IEnumerable<TEntity> entityEnumerable,
        TEntityId entityId)
        where TEntityId : struct
        where TEntity : Entity<TEntityId>
    {
        TEntity? entity = entityEnumerable.FirstOrDefault(entity => entity.Id.Equals(entityId));
        return Maybe<TEntity>.From(entity);
    }
}

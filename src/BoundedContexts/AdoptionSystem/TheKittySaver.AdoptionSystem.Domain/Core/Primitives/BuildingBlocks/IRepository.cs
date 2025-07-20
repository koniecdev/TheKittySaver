using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.OptionMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;

public interface IRepository<TAggregateRoot, in TAggregateRootId> 
    where TAggregateRootId : struct
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
{
    Task<Maybe<TAggregateRoot>> GetByIdAsync(TAggregateRootId id, CancellationToken cancellationToken);
    void Insert(TAggregateRoot entity);
    void Remove(TAggregateRoot entity);
}
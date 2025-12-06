using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

public interface IRepository<TAggregateRoot, in TAggregateRootId> 
    where TAggregateRootId : struct
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
{
    Task<Maybe<TAggregateRoot>> GetByIdAsync(TAggregateRootId id, CancellationToken cancellationToken);
    void Insert(TAggregateRoot aggregate);
    void Remove(TAggregateRoot aggregate);
}

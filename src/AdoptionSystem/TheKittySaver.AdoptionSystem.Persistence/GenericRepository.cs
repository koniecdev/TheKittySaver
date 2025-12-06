using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Common;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Persistence;

internal abstract class GenericRepository<TAggregateRoot, TAggregateRootId> : IRepository<TAggregateRoot, TAggregateRootId>
    where TAggregateRootId : struct, IStronglyTypedId<TAggregateRootId>
    where TAggregateRoot : AggregateRoot<TAggregateRootId>
{
    protected readonly ApplicationWriteDbContext DbContext;

    protected GenericRepository(ApplicationWriteDbContext db)
    {
        DbContext = db;
    }

    public virtual async Task<Maybe<TAggregateRoot>> GetByIdAsync(
        TAggregateRootId id,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(id);
        
        TAggregateRoot? entity = await DbContext.Set<TAggregateRoot>()
            .FirstOrDefaultAsync(x => x.Id.Equals(id), cancellationToken);

        return Maybe<TAggregateRoot>.From(entity);
    }

    public void Insert(TAggregateRoot aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        
        DbContext.Set<TAggregateRoot>().Add(aggregate);
    }

    public void Remove(TAggregateRoot aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        
        DbContext.Set<TAggregateRoot>().Remove(aggregate);
    }
}

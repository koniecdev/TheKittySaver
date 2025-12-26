using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Persistence.DomainRepositories;

internal sealed class PersonRepository : GenericRepository<Person, PersonId>, IPersonRepository
{
    public PersonRepository(ApplicationWriteDbContext db) : base(db)
    {
    }

    public override async Task<Maybe<Person>> GetByIdAsync(
        PersonId id,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(id);
        
        Person? result = await DbContext.Persons
            .Where(x => x.Id == id)
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(cancellationToken);
        
        return Maybe<Person>.From(result);
    }

    public async Task<Maybe<Person>> GetByIdentityIdAsync(IdentityId identityId, CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(identityId);
        
        Person? result = await DbContext.Persons
            .Where(x => x.IdentityId == identityId)
            .Include(x => x.Addresses)
            .FirstOrDefaultAsync(cancellationToken);
        
        return Maybe<Person>.From(result);
    }
}

using TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.Repositories;

public interface IPersonRepository : IRepository<Person, PersonId>
{
    Task<Maybe<Person>> GetByIdentityIdAsync(IdentityId identityId, CancellationToken cancellationToken);
}

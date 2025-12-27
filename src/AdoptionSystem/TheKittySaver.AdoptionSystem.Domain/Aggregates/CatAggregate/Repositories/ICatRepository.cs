using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;

public interface ICatRepository : IRepository<Cat, CatId>
{
    public Task<IReadOnlyCollection<Cat>> GetCatsByAdoptionAnnouncementIdAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        CancellationToken cancellationToken);

    public Task<IReadOnlyCollection<Cat>> GetCatsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken);
    
    public Task<IReadOnlyCollection<Cat>> GetArchivedCatsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken);

    public Task<int> CountCatsByAdoptionAnnouncementIdAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        CancellationToken cancellationToken);

    public Task<int> CountUnclaimedCatsByAdoptionAnnouncementIdAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        CancellationToken cancellationToken);

    public Task<Maybe<Cat>> GetArchivedByIdAsync(
        CatId catId,
        CancellationToken cancellationToken);

    public Task<int> CountCatsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken);
}

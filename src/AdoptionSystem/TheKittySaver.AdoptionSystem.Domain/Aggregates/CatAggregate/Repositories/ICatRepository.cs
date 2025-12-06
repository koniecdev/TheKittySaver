using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;

public interface ICatRepository : IRepository<Cat, CatId>
{
    public Task<IReadOnlyCollection<Cat>> GetCatsByAdoptionAnnouncementIdAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        CancellationToken cancellationToken);
}

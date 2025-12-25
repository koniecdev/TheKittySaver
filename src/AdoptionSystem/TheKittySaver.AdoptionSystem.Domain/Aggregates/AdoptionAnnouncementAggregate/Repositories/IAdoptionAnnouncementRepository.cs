using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;

public interface IAdoptionAnnouncementRepository : IRepository<AdoptionAnnouncement, AdoptionAnnouncementId>
{
    public Task<IReadOnlyCollection<AdoptionAnnouncement>> GetAdoptionAnnouncementsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken);
}

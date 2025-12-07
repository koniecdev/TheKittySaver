using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Persistence.DomainRepositories;

internal sealed class AdoptionAnnouncementRepository : 
    GenericRepository<AdoptionAnnouncement, AdoptionAnnouncementId>, IAdoptionAnnouncementRepository
{
    public AdoptionAnnouncementRepository(ApplicationWriteDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyCollection<AdoptionAnnouncement>> GetAdoptionAnnouncementByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(personId);
        
        List<AdoptionAnnouncement> adopAnnouncements = await DbContext.AdoptionAnnouncements
            .Where(x => x.PersonId == personId)
            .ToListAsync(cancellationToken);
        
        return adopAnnouncements;
    }
}

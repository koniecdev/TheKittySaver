using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;

namespace TheKittySaver.AdoptionSystem.Persistence.DomainRepositories;

internal sealed class AdoptionAnnouncementRepository : 
    GenericRepository<AdoptionAnnouncement, AdoptionAnnouncementId>, IAdoptionAnnouncementRepository
{
    public AdoptionAnnouncementRepository(ApplicationWriteDbContext db) : base(db)
    {
    }

    public override async Task<Maybe<AdoptionAnnouncement>> GetByIdAsync(
        AdoptionAnnouncementId id,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(id);
        
        AdoptionAnnouncement? result = await DbContext.AdoptionAnnouncements
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        
        return Maybe<AdoptionAnnouncement>.From(result);
    }
}

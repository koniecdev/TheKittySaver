using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.AdoptionAnnouncementAggregate.Repositories;
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
}

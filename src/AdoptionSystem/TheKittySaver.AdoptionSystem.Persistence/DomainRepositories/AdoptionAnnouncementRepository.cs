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

    public async Task<IReadOnlyCollection<AdoptionAnnouncement>> GetAdoptionAnnouncementsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(personId);

        List<AdoptionAnnouncement> adopAnnouncements = await DbContext.AdoptionAnnouncements
            .Where(x => x.PersonId == personId)
            .ToListAsync(cancellationToken);

        return adopAnnouncements;
    }

    public async Task<IReadOnlyCollection<AdoptionAnnouncement>> GetArchivedAnnouncementsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(personId);

        List<AdoptionAnnouncement> adopAnnouncements = await DbContext.AdoptionAnnouncements
            .IgnoreQueryFilters()
            .Where(x => x.PersonId == personId && x.ArchivedAt != null)
            .ToListAsync(cancellationToken);

        return adopAnnouncements;
    }

    public async Task<Maybe<AdoptionAnnouncement>> GetArchivedByIdAsync(
        AdoptionAnnouncementId announcementId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(announcementId);

        AdoptionAnnouncement? result = await DbContext.AdoptionAnnouncements
            .IgnoreQueryFilters()
            .Where(x => x.Id == announcementId && x.ArchivedAt != null)
            .FirstOrDefaultAsync(cancellationToken);

        return Maybe<AdoptionAnnouncement>.From(result);
    }

    public async Task<int> CountAnnouncementsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(personId);

        int count = await DbContext.AdoptionAnnouncements
            .CountAsync(x => x.PersonId == personId, cancellationToken);

        return count;
    }
}

using Microsoft.EntityFrameworkCore;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Repositories;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.OptionMonad;
using TheKittySaver.AdoptionSystem.Persistence.DbContexts.WriteDbContexts;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.AdoptionAnnouncementAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.PersonAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Persistence.DomainRepositories;

internal sealed class CatRepository : GenericRepository<Cat, CatId>, ICatRepository
{
    public CatRepository(ApplicationWriteDbContext db) : base(db)
    {
    }
    
    public override async Task<Maybe<Cat>> GetByIdAsync(
        CatId id,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(id);
        
        Cat? result = await DbContext.Cats
            .Where(x => x.Id == id)
            .Include(cat => cat.Thumbnail)
            .Include(cat => cat.GalleryItems)
            .Include(cat => cat.Vaccinations)
            .FirstOrDefaultAsync(cancellationToken);
        
        return Maybe<Cat>.From(result);
    }

    public async Task<IReadOnlyCollection<Cat>> GetCatsByAdoptionAnnouncementIdAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(adoptionAnnouncementId);
        
        IReadOnlyCollection<Cat> result = await DbContext.Cats
            .Where(x => x.AdoptionAnnouncementId == adoptionAnnouncementId)
            .Include(cat => cat.Thumbnail)
            .Include(cat => cat.GalleryItems)
            .Include(cat => cat.Vaccinations)
            .ToListAsync(cancellationToken);
        
        return result;
    }

    public async Task<IReadOnlyCollection<Cat>> GetCatsByPersonIdAsync(
        PersonId personId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(personId);

        List<Cat> result = await DbContext.Cats
            .Where(x => x.PersonId == personId)
            .Include(cat => cat.Thumbnail)
            .Include(cat => cat.GalleryItems)
            .Include(cat => cat.Vaccinations)
            .ToListAsync(cancellationToken);

        return result;
    }

    public async Task<int> CountCatsByAdoptionAnnouncementIdAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(adoptionAnnouncementId);

        int count = await DbContext.Cats
            .CountAsync(x => x.AdoptionAnnouncementId == adoptionAnnouncementId, cancellationToken);

        return count;
    }

    public async Task<int> CountUnclaimedCatsByAdoptionAnnouncementIdAsync(
        AdoptionAnnouncementId adoptionAnnouncementId,
        CancellationToken cancellationToken)
    {
        Ensure.NotEmpty(adoptionAnnouncementId);

        int count = await DbContext.Cats
            .CountAsync(
                x => x.AdoptionAnnouncementId == adoptionAnnouncementId
                     && x.Status != CatStatusType.Claimed,
                cancellationToken);

        return count;
    }
}

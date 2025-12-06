using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class CatThumbnail : Entity<CatThumbnailId>
{
    public CatId CatId { get; }
    
    public static Result<CatThumbnail> Create(CatId catId)
    {
        Ensure.NotEmpty(catId);
        
        CatThumbnailId id = CatThumbnailId.New();
        CatThumbnail instance = new(catId, id);
        return Result.Success(instance);
    }

    private CatThumbnail(CatId catId, CatThumbnailId id) : base(id)
    {
        CatId = catId;
    }

    private CatThumbnail()
    {
    }
}

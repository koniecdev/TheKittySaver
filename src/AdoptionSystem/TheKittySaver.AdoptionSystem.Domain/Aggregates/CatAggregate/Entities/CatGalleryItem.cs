using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;
using TheKittySaver.AdoptionSystem.Primitives.Guards;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;

public sealed class CatGalleryItem : Entity<CatGalleryItemId>
{
    public CatId CatId { get; }
    public CatGalleryItemDisplayOrder DisplayOrder { get; private set; }

    internal Result UpdateDisplayOrder(CatGalleryItemDisplayOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
        DisplayOrder = order;
        return Result.Success();
    }
    
    internal static Result<CatGalleryItem> Create(
        CatId catId,
        CatGalleryItemDisplayOrder order)
    {
        Ensure.NotEmpty(catId);
        ArgumentNullException.ThrowIfNull(order);
        CatGalleryItemId id = CatGalleryItemId.Create();
        CatGalleryItem instance = new(catId, id, order);
        return Result.Success(instance);
    }

    private CatGalleryItem(
        CatId catId,
        CatGalleryItemId id,
        CatGalleryItemDisplayOrder displayOrder) : base(id)
    {
        CatId = catId;
        DisplayOrder = displayOrder;
    }
    
    private CatGalleryItem()
    {
        DisplayOrder = null!;
    }
}

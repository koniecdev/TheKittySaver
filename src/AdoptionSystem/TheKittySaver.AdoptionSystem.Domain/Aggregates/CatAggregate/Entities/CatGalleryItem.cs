using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Guards;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

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
        CatGalleryItemDisplayOrder order,
        CreatedAt createdAt)
    {
        Ensure.NotEmpty(catId);
        ArgumentNullException.ThrowIfNull(order);
        CatGalleryItemId id = CatGalleryItemId.New();
        CatGalleryItem instance = new(catId, id, order, createdAt);
        return Result.Success(instance);
    }

    private CatGalleryItem(
        CatId catId,
        CatGalleryItemId id,
        CatGalleryItemDisplayOrder displayOrder,
        CreatedAt createdAt) : base(id, createdAt)
    {
        CatId = catId;
        DisplayOrder = displayOrder;
    }
    
    private CatGalleryItem()
    {
        DisplayOrder = null!;
    }
}

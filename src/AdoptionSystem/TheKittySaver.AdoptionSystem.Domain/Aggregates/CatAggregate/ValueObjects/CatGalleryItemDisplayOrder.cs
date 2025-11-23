using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatGalleryItemDisplayOrder : ValueObject
{
    public const int MinValue = 0;
    public int Value { get; }

    public static Result<CatGalleryItemDisplayOrder> Create(int imageOrder, int maxAllowedOrder)
    {
        if (imageOrder < MinValue)
        {
            return Result.Failure<CatGalleryItemDisplayOrder>(
                DomainErrors.CatGalleryItem.DisplayOrder.BelowMinimum(imageOrder, MinValue));
        }

        if (imageOrder >= maxAllowedOrder)
        {
            return Result.Failure<CatGalleryItemDisplayOrder>(
                DomainErrors.CatGalleryItem.DisplayOrder.AboveOrEqualMaximum(
                    imageOrder, maxAllowedOrder));
        }

        CatGalleryItemDisplayOrder instance = new(imageOrder);
        return Result.Success(instance);
    }

    private CatGalleryItemDisplayOrder(int value)
    {
        Value = value;
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
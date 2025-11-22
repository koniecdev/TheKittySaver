using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatThumbnail : ValueObject
{
    public CatImageId CatImageId { get; }

    public static Result<CatThumbnail> Create()
    {
        CatImageId catImageId = CatImageId.New();
        CatThumbnail instance = new(catImageId);
        return Result.Success(instance);
    }

    private CatThumbnail(CatImageId catImageId)
    {
        CatImageId = catImageId;
    }

    public override string ToString() => CatImageId.ToString();
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CatImageId;
    }
}
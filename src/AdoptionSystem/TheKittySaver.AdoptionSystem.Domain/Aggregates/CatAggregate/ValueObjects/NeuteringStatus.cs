using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class NeuteringStatus : ValueObject
{
    public bool IsNeutered { get; }
    
    public static NeuteringStatus NotNeutered() => new(false);
    
    public static NeuteringStatus Neutered() => new(true);

    private NeuteringStatus(bool isNeutered)
    {
        IsNeutered = isNeutered;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return IsNeutered;
    }
}
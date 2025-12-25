using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class Temperament : ValueObject
{
    public TemperamentType Value { get; }
    
    public static Temperament Friendly() => new(TemperamentType.Friendly);
    public static Temperament Independent() => new(TemperamentType.Independent);
    public static Temperament Timid() => new(TemperamentType.Timid);
    public static Temperament VeryTimid() => new(TemperamentType.VeryTimid);
    public static Temperament Aggressive() => new(TemperamentType.Aggressive);
    
    private Temperament(TemperamentType value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
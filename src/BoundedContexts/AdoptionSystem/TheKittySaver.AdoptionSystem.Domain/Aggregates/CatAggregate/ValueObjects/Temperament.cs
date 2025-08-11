using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class Temperament : ValueObject
{
    public enum TemperamentType
    {
        Unset,
        Friendly,
        Independent,
        Timid,
        VeryTimid,
        Aggressive
    }
    
    public TemperamentType Value { get; }
    
    public AdoptionPriorityScore CalculatePriorityScore()
    {
        decimal points = Value switch
        {
            TemperamentType.Aggressive => 15,
            TemperamentType.VeryTimid => 12,
            TemperamentType.Timid => 8,
            TemperamentType.Independent => 5,
            _ => 0
        };
        
        Result<AdoptionPriorityScore> result = AdoptionPriorityScore.Create(points);
        
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException("Something went wrong while calculating priority points");
    }
    
    public static Temperament Friendly() => new(TemperamentType.Friendly);
    public static Temperament Independent() => new(TemperamentType.Independent);
    public static Temperament Timid() => new(TemperamentType.Timid);
    public static Temperament VeryTimid() => new(TemperamentType.VeryTimid);
    public static Temperament Aggressive() => new(TemperamentType.Aggressive);
    
    private Temperament(TemperamentType value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
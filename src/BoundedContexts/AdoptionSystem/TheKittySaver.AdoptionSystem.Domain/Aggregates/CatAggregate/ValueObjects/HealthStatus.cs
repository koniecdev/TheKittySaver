using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class HealthStatus : ValueObject
{
    public enum StatusType
    {
        Unset,
        Healthy,
        MinorIssues,
        Recovering,
        ChronicIllness,
        Critical
    }
    
    public StatusType Value { get; }
    
    public AdoptionPriorityScore CalculatePriorityScore()
    {
        decimal points = Value switch
        {
            StatusType.Critical => 40,
            StatusType.ChronicIllness => 35,
            StatusType.Recovering => 25,
            StatusType.MinorIssues => 15,
            _ => 0
        };
        
        Result<AdoptionPriorityScore> result = AdoptionPriorityScore.Create(points);
        
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException("Something went wrong while calculating priority points");
    }
    
    public static HealthStatus Healthy() => new(StatusType.Healthy);
    public static HealthStatus MinorIssues() => new(StatusType.MinorIssues);
    public static HealthStatus Recovering() => new(StatusType.Recovering);
    public static HealthStatus ChronicIllness() => new(StatusType.ChronicIllness);
    public static HealthStatus Critical() => new(StatusType.Critical);
    
    private HealthStatus(StatusType value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
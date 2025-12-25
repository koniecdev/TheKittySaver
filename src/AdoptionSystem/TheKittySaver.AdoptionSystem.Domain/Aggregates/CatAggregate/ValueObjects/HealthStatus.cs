using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class HealthStatus : ValueObject
{
    public HealthStatusType Value { get; }
    
    public static HealthStatus Healthy() => new(HealthStatusType.Healthy);
    public static HealthStatus MinorIssues() => new(HealthStatusType.MinorIssues);
    public static HealthStatus Recovering() => new(HealthStatusType.Recovering);
    public static HealthStatus ChronicIllness() => new(HealthStatusType.ChronicIllness);
    public static HealthStatus Critical() => new(HealthStatusType.Critical);
    
    private HealthStatus(HealthStatusType value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class BuildingNumber : ValueObject
{
    public const int MaxLength = 10;
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(BuildingNumber value) => value.Value;
    
    public static Result<BuildingNumber> Create(string value)
    {
        var result = Result.Create(value, DomainErrors.PolishAddressEntity.BuildingNumberProperty.NullOrEmpty)
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.PolishAddressEntity.BuildingNumberProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.PolishAddressEntity.BuildingNumberProperty.LongerThanAllowed)
            .Map(v => new BuildingNumber(v));
        return result;
    }

    private BuildingNumber(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
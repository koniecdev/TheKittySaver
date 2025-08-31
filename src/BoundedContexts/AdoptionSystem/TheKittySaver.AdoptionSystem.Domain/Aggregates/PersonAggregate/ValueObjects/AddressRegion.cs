using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class AddressRegion : ValueObject
{
    public const int MaxLength = 100;
    public string Value { get; }
    
    public static Result<AddressRegion> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AddressRegion>(DomainErrors.AddressEntity.RegionProperty.NullOrEmpty);
        }

        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<AddressRegion>(DomainErrors.AddressEntity.RegionProperty.LongerThanAllowed);
        }

        AddressRegion instance = new(value);
        return Result.Success(instance);
    }

    private AddressRegion(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(AddressRegion value) => value.Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
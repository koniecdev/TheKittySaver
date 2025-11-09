using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.AddressCompounds;

public sealed class AddressRegion : ValueObject
{
    public const int MaxLength = 200;
    public string Value { get; }
    
    public static Result<AddressRegion> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AddressRegion>(DomainErrors.AddressRegionValueObject.NullOrEmpty);
        }

        value = value.Trim();

        if (value.Length > MaxLength)
        {
            return Result.Failure<AddressRegion>(DomainErrors.AddressRegionValueObject.LongerThanAllowed);
        }

        AddressRegion instance = new(value);
        return Result.Success(instance);
    }

    private AddressRegion(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
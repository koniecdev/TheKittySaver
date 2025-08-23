using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed class BuildingNumber : ValueObject
{
    public const int MaxLength = 10;
    
    public string Value { get; }
    
    
    public static Result<BuildingNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<BuildingNumber>(
                DomainErrors.PolishAddressEntity.BuildingNumberProperty.NullOrEmpty);
        }
        
        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<BuildingNumber>(
                DomainErrors.PolishAddressEntity.BuildingNumberProperty.LongerThanAllowed);
        }
        
        BuildingNumber instance = new(value);
        return Result.Success(instance);
    }

    private BuildingNumber(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(BuildingNumber value) => value.Value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
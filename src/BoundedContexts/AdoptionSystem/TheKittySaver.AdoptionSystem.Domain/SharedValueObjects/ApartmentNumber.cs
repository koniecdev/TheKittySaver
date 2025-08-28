using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed class ApartmentNumber : ValueObject
{
    public const int MaxLength = 10;
    
    public string Value { get; }
    
    public static Result<ApartmentNumber> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<ApartmentNumber>(
                DomainErrors.PolishAddressEntity.ApartmentNumberProperty.NullOrEmpty);
        }
        
        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<ApartmentNumber>(
                DomainErrors.PolishAddressEntity.ApartmentNumberProperty.LongerThanAllowed);
        }
        
        ApartmentNumber instance = new(value);
        return Result.Success(instance);
    }
    
    private ApartmentNumber(string value)
    {
        Value = value;
    }
    
    public override string ToString() => Value;
    public static implicit operator string(ApartmentNumber value) => value.Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
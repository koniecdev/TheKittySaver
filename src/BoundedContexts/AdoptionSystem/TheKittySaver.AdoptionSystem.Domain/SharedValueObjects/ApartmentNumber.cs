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
        
        string trimmedValue = value.Trim();
        
        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<ApartmentNumber>(
                DomainErrors.PolishAddressEntity.ApartmentNumberProperty.LongerThanAllowed);
        }
        
        ApartmentNumber instance = new(trimmedValue);
        return Result.Success(instance);
    }
    
    private ApartmentNumber(string value)
    {
        Value = value;
    }

    public static implicit operator string(ApartmentNumber value) => value.Value;
    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
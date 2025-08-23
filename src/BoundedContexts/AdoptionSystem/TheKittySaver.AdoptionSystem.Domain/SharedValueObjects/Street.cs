using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed class Street : ValueObject
{
    public const int MaxLength = 200;
    public string Value { get; }
    
    public static Result<Street> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Street>(DomainErrors.PolishAddressEntity.StreetProperty.NullOrEmpty);
        }
        
        string trimmedValue = value.Trim();
        
        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<Street>(DomainErrors.PolishAddressEntity.StreetProperty.LongerThanAllowed);
        }
        
        Street instance = new(trimmedValue);
        return Result.Success(instance);
    }

    private Street(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(Street value) => value.Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
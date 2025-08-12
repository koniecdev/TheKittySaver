using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.SharedValueObjects;

public sealed class City : ValueObject
{
    public const int MaxLength = 100;
    public string Value { get; }
    
    public static Result<City> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<City>(DomainErrors.PolishAddressEntity.CityProperty.NullOrEmpty);
        }
        
        string trimmedValue = value.Trim();
        
        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<City>(DomainErrors.PolishAddressEntity.CityProperty.LongerThanAllowed);
        }
        
        City instance = new(trimmedValue);
        return Result.Success(instance);
    }

    private City(string value)
    {
        Value = value;
    }
    
    public static implicit operator string(City value) => value.Value;
    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
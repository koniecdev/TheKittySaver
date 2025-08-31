using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class AddressCity : ValueObject
{
    public const int MaxLength = 100;
    
    public string Value { get; }
    
    public static Result<AddressCity> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AddressCity>(DomainErrors.AddressEntity.CityProperty.NullOrEmpty);
        }
        
        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<AddressCity>(DomainErrors.AddressEntity.CityProperty.LongerThanAllowed);
        }
        
        AddressCity instance = new(value);
        return Result.Success(instance);
    }

    private AddressCity(string value)
    {
        Value = value;
    }
    
    public override string ToString() => Value;
    public static implicit operator string(AddressCity value) => value.Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
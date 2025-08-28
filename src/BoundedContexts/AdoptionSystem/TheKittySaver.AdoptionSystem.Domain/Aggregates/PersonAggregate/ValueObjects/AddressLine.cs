using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class AddressLine : ValueObject
{
    public const int MaxLength = 500;
    public string Value { get; }
    
    public static Result<AddressLine> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AddressLine>(DomainErrors.PolishAddressEntity.NameProperty.NullOrEmpty);
        }

        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<AddressLine>(DomainErrors.PolishAddressEntity.NameProperty.LongerThanAllowed);
        }

        AddressLine instance = new(value);
        return Result.Success(instance);
    }

    private AddressLine(string value)
    {
        Value = value;
    }

    public static implicit operator string(AddressLine value) => value.Value;
    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
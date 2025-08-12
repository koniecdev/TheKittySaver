using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class AddressName : ValueObject
{
    public const int MaxLength = 150;
    public string Value { get; }
    
    public static Result<AddressName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<AddressName>(DomainErrors.PolishAddressEntity.NameProperty.NullOrEmpty);
        }

        string trimmedValue = value.Trim();
        
        if (trimmedValue.Length > MaxLength)
        {
            return Result.Failure<AddressName>(DomainErrors.PolishAddressEntity.NameProperty.LongerThanAllowed);
        }

        AddressName instance = new(trimmedValue);
        return Result.Success(instance);
    }

    private AddressName(string value)
    {
        Value = value;
    }

    public static implicit operator string(AddressName value) => value.Value;
    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
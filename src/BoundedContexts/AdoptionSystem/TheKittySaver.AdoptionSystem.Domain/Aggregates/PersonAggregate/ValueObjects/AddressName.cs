using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class AddressName : ValueObject
{
    public const int MaxLength = 150;
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(AddressName value) => value.Value;
    
    public static Result<AddressName> Create(string value)
    {
        Result<AddressName> result = Result.Create(value, DomainErrors.PolishAddressEntity.NameProperty.NullOrEmpty)
            .TrimValue()
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.PolishAddressEntity.NameProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.PolishAddressEntity.NameProperty.LongerThanAllowed)
            .Map(v => new AddressName(v));
        return result;
    }

    private AddressName(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
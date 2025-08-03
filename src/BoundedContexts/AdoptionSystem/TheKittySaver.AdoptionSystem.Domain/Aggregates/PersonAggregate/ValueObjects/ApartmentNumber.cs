using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class ApartmentNumber : ValueObject
{
    public const int MaxLength = 10;
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(ApartmentNumber value) => value.Value;
    
    public static Result<ApartmentNumber> Create(string value)
    {
        Result<ApartmentNumber> result = 
            Result.Create(value, DomainErrors.PolishAddressEntity.ApartmentNumberProperty.NullOrEmpty)
                .TrimValue()
                .Ensure(v => !string.IsNullOrWhiteSpace(v),
                    DomainErrors.PolishAddressEntity.ApartmentNumberProperty.NullOrEmpty)
                .Ensure(v =>
                    v.Length <= MaxLength, DomainErrors.PolishAddressEntity.ApartmentNumberProperty.LongerThanAllowed)
                .Map(v => new ApartmentNumber(v));
        return result;
    }
    
    private ApartmentNumber(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
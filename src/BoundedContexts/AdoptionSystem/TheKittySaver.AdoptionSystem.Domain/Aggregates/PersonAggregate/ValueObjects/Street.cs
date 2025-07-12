using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class Street : ValueObject
{
    public const int MaxLength = 200;
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(Street value) => value.Value;
    
    public static Result<Street> Create(string value)
    {
        var result = Result.Create(value, DomainErrors.PolishAddressEntity.StreetProperty.NullOrEmpty)
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.PolishAddressEntity.StreetProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.PolishAddressEntity.StreetProperty.LongerThanAllowed)
            .Map(v => new Street(v));
        return result;
    }

    private Street(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
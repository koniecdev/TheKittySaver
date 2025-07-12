using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class PhoneNumber : ValueObject
{
    public const int MaxLength = 30;
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(PhoneNumber value) => value.Value;
    
    public static Result<PhoneNumber> Create(string value) =>
        Result.Create(value, DomainErrors.PersonEntity.PhoneNumberProperty.NullOrEmpty)
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.PersonEntity.PhoneNumberProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.PersonEntity.PhoneNumberProperty.LongerThanAllowed)
            .Map(v => new PhoneNumber(v));
    
    private PhoneNumber(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.Results;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ValueObjects;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class Username : ValueObject
{
    public const int MaxLength = 150;
    public string Value { get; }
    
    public override string ToString() => Value;
    public static implicit operator string(Username value) => value.Value;
    
    public static Result<Username> Create(string value) =>
        Result.Create(value, DomainErrors.PersonEntity.EmailProperty.NullOrEmpty)
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.PersonEntity.UsernameProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.PersonEntity.UsernameProperty.LongerThanAllowed)
            .Map(v => new Username(v));
    
    private Username(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
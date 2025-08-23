using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.PersonAggregate.ValueObjects;

public sealed class Username : ValueObject
{
    public const int MaxLength = 150;
    public string Value { get; }
    
    public static Result<Username> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<Username>(DomainErrors.PersonEntity.UsernameProperty.NullOrEmpty);
        }

        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<Username>(DomainErrors.PersonEntity.UsernameProperty.LongerThanAllowed);
        }

        Username instance = new(value);
        return Result.Success(instance);
    }
    
    private Username(string value)
    {
        Value = value;
    }

    public static implicit operator string(Username value) => value.Value;
    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
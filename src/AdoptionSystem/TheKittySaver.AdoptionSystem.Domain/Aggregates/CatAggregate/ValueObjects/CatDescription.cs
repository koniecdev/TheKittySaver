using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatDescription : ValueObject
{
    public const int MaxLength = 250;
    public string Value { get; }

    public static Result<CatDescription> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CatDescription>(DomainErrors.Cat.Description.NullOrEmpty);
        }

        if (value.Length > MaxLength)
        {
            return Result.Failure<CatDescription>(DomainErrors.Cat.Description.LongerThanAllowed);
        }
        
        CatDescription instance = new(value);
        return Result.Success(instance);
    }

    private CatDescription(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
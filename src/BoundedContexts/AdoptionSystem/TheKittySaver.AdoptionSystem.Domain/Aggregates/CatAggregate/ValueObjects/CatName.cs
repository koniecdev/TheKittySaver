using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatName : ValueObject
{
    public const int MaxLength = 50;
    public string Value { get; }
    
    public static Result<CatName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<CatName>(DomainErrors.CatEntity.CatNameProperty.NullOrEmpty);
        }
        
        value = value.Trim();
        
        if (value.Length > MaxLength)
        {
            return Result.Failure<CatName>(DomainErrors.CatEntity.CatNameProperty.LongerThanAllowed);
        }

        CatName instance = new(value);
        return Result.Success(instance);
    }

    private CatName(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;
    public static implicit operator string(CatName value) => value.Value;
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
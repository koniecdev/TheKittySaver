using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatName : ValueObject
{
    public const int MaxLength = 50;
    public string Value { get; }

    public override string ToString() => Value;
    public static implicit operator string(CatName value) => value.Value;

    public static Result<CatName> Create(string value)
    {
        Result<CatName> result = Result.Create(value, DomainErrors.CatEntity.CatNameProperty.NullOrEmpty)
            .TrimValue()
            .Ensure(v => !string.IsNullOrWhiteSpace(v), DomainErrors.CatEntity.CatNameProperty.NullOrEmpty)
            .Ensure(v => v.Length <= MaxLength, DomainErrors.CatEntity.CatNameProperty.LongerThanAllowed)
            .Map(v => new CatName(v));
        return result;
    }

    private CatName(string value) => Value = value;

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
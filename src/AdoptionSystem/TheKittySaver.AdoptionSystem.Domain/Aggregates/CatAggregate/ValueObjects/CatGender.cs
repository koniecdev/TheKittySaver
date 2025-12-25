using TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Primitives.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatGender : ValueObject
{
    public CatGenderType Value { get; }
    
    public bool IsMale => Value is CatGenderType.Male;
    public bool IsFemale => Value is CatGenderType.Female;
    
    public static CatGender Male() => new(CatGenderType.Male);
    public static CatGender Female() => new(CatGenderType.Female);
    
    private CatGender(CatGenderType value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToString();

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
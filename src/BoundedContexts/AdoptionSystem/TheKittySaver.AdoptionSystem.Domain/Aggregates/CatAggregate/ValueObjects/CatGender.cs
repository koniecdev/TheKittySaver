using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class CatGender : ValueObject
{
    public enum GenderType
    {
        Unset,
        Male,
        Female
    }
    
    public GenderType Value { get; }
    
    
    public AdoptionPriorityScore CalculatePriorityScore()
    {
        decimal points = Value switch
        {
            GenderType.Male => 5,
            _ => 0
        };
        
        Result<AdoptionPriorityScore> result = AdoptionPriorityScore.Create(points);
        
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException("Something went wrong while calculating priority points");
    }
    
    public bool IsMale => Value is GenderType.Male;
    public bool IsFemale => Value is GenderType.Female;
    
    public static CatGender Male() => new(GenderType.Male);
    public static CatGender Female() => new(GenderType.Female);
    
    public static Result<CatGender> Create(bool isMale)
    {
        GenderType type = isMale ? GenderType.Male : GenderType.Female;
        CatGender instance = new(type);
        return Result.Success(instance);
    }
    
    private CatGender(GenderType value)
    {
        Value = value;
    }
    
    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
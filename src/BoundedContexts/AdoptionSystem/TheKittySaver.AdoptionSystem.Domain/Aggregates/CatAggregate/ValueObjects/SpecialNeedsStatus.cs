using TheKittySaver.AdoptionSystem.Domain.Core.Errors;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public sealed class SpecialNeedsStatus : ValueObject
{
    public enum SpecialNeedsSeverityType
    {
        Unset,
        None,
        Minor,
        Moderate,
        Severe
    }
    
    public const int MaxDescriptionLength = 500;
    
    public bool HasSpecialNeeds { get; }
    public string? Description { get; }
    public SpecialNeedsSeverityType SeverityType { get; }
    
    public AdoptionPriorityScore CalculatePriorityPoints()
    {
        int points = SeverityType switch
        {
            SpecialNeedsSeverityType.Severe => 25, // np. paraliż, ślepota
            SpecialNeedsSeverityType.Moderate => 15, // np. brak oka, FIV+
            SpecialNeedsSeverityType.Minor => 8, // np. alergia pokarmowa
            SpecialNeedsSeverityType.None => 0,
            _ => 0
        };

        Result<AdoptionPriorityScore> result = AdoptionPriorityScore.Create(points);
        
        return result.IsSuccess
            ? result.Value
            : throw new InvalidOperationException("Something went wrong while calculating priority points");
    }
    
    public static SpecialNeedsStatus None() => new(false, null, SpecialNeedsSeverityType.None);
    public static Result<SpecialNeedsStatus> Create(string description, SpecialNeedsSeverityType severityType)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Result.Failure<SpecialNeedsStatus>(
                DomainErrors.CatEntity.SpecialNeedsStatusProperty.DescriptionIsNullOrEmpty);
        }

        if (severityType is SpecialNeedsSeverityType.Unset)
        {
            return Result.Failure<SpecialNeedsStatus>(
                DomainErrors.CatEntity.SpecialNeedsStatusProperty.SpecialNeedsSeverityIsUnset);
        }
        
        description = description.Trim();

        if (description.Length > MaxDescriptionLength)
        {
            return Result.Failure<SpecialNeedsStatus>(
                DomainErrors.CatEntity.SpecialNeedsStatusProperty.DescriptionIsLongerThanAllowed);
        }

        SpecialNeedsStatus instance = new(true, description, severityType);
        return Result.Success(instance);
    }

    private SpecialNeedsStatus(bool hasSpecialNeeds, string? description, SpecialNeedsSeverityType severityType)
    {
        HasSpecialNeeds = hasSpecialNeeds;
        Description = description;
        SeverityType = severityType;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return HasSpecialNeeds;
        yield return SeverityType;
        
        if (Description is not null)
        {
            yield return Description;
        }
    }
}

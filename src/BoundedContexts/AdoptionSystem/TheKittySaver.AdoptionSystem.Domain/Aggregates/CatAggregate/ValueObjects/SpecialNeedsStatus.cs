using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Enums;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;

public record SpecialNeedsStatus
{
    public bool HasSpecialNeeds { get; }
    public string? Description { get; }
    public SpecialNeedsSeverity Severity { get; }
    
    private SpecialNeedsStatus(bool hasSpecialNeeds, string? description, SpecialNeedsSeverity severity)
    {
        HasSpecialNeeds = hasSpecialNeeds;
        Description = description;
        Severity = severity;
    }
    
    public static SpecialNeedsStatus None() => new(false, null, SpecialNeedsSeverity.None);
    
    public static SpecialNeedsStatus Create(string description, SpecialNeedsSeverity severity) 
        => new(true, description, severity);
    
    public decimal CalculatePriorityPoints() => Severity switch
    {
        SpecialNeedsSeverity.Severe => 25,      // np. paraliż, ślepota
        SpecialNeedsSeverity.Moderate => 15,    // np. brak oka, FIV+
        SpecialNeedsSeverity.Minor => 8,        // np. alergia pokarmowa
        SpecialNeedsSeverity.None => 0,
        _ => 0
    };
}
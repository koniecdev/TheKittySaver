namespace TheKittySaver.AdoptionSystem.Calculators;

public sealed class CalculatorsOptions
{
    public const string SectionName = "Calculators";
    public string AdoptionPriorityScoreCalculator { get; init; } = "Default";
}

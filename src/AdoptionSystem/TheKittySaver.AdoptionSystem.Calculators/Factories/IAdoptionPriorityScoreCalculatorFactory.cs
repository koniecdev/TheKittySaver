using TheKittySaver.AdoptionSystem.Calculators.Abstractions;

namespace TheKittySaver.AdoptionSystem.Calculators.Factories;

public interface IAdoptionPriorityScoreCalculatorFactory
{
    public IAdoptionPriorityScoreCalculator Create();
}

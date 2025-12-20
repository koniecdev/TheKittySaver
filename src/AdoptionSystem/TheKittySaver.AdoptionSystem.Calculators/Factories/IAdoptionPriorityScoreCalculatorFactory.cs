using Microsoft.Extensions.Options;
using TheKittySaver.AdoptionSystem.Calculators.Abstractions;
using TheKittySaver.AdoptionSystem.Calculators.CatPriorityScore;

namespace TheKittySaver.AdoptionSystem.Calculators.Factories;

public interface IAdoptionPriorityScoreCalculatorFactory
{
    public IAdoptionPriorityScoreCalculator CreateAdoptionPriorityScoreCalculator();
}

internal sealed class AdoptionPriorityScoreCalculatorFactory : IAdoptionPriorityScoreCalculatorFactory
{
    private readonly IOptionsMonitor<CalculatorsOptions> _optionsMonitor;

    public AdoptionPriorityScoreCalculatorFactory(IOptionsMonitor<CalculatorsOptions> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
    }
    
    public IAdoptionPriorityScoreCalculator CreateAdoptionPriorityScoreCalculator()
    {
        string implementationName = _optionsMonitor.CurrentValue.AdoptionPriorityScoreCalculator;
        IAdoptionPriorityScoreCalculator calculator = implementationName switch
        {
            _ => new DefaultAdoptionPriorityScoreCalculator()
        };
        
        return calculator;
    }
}

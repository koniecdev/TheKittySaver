using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.ValueObjects;
using TheKittySaver.AdoptionSystem.Domain.Core.Primitives.ResultMonad;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;

public sealed class DefaultAdoptionPriorityScoreCalculator
{
    public Result<AdoptionPriorityScore> Calculate(Cat cat)
    {
        var adoptionHistoryPoints = 
    }
}
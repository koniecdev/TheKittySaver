using TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Entities;
using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Aggregates.CatAggregate.Services;

public interface ICatArchiveDomainService
{
    Result Archive(Cat cat, ArchivedAt archivedAt);
    Result Unarchive(Cat cat);
}

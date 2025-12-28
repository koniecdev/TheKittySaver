using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;

public interface IArchivable
{
    public ArchivedAt? ArchivedAt { get; }
}

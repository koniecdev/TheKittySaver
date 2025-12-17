using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;

public interface IPublishable
{
    public PublishedAt? PublishedAt { get; }
}

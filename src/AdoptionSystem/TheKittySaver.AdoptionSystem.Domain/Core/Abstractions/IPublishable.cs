using TheKittySaver.AdoptionSystem.Domain.Core.Monads.ResultMonad;
using TheKittySaver.AdoptionSystem.Domain.SharedValueObjects.Timestamps;

namespace TheKittySaver.AdoptionSystem.Domain.Core.Abstractions;

public interface IPublishable
{
    public PublishedAt? PublishedAt { get; }

    public Result Publish(PublishedAt publishedAt);

    public Result Unpublish();
}

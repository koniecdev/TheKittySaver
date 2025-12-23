using TheKittySaver.AdoptionSystem.Primitives.Common;

namespace TheKittySaver.AdoptionSystem.ReadModels.Core.BuildingBlocks;

public interface IReadOnlyEntity<out T> where T : struct, IStronglyTypedId<T>
{
    public T Id { get; }
    public DateTimeOffset CreatedAt { get; }
}

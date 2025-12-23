namespace TheKittySaver.AdoptionSystem.Primitives.Common;

public interface IStronglyTypedId<out TSelf> where TSelf : IStronglyTypedId<TSelf>
{
    public Guid Value { get; }

    public static abstract TSelf Create();
    public static abstract TSelf Create(Guid id);
}

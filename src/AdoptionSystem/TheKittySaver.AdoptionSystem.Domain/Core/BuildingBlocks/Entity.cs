namespace TheKittySaver.AdoptionSystem.Domain.Core.BuildingBlocks;

public interface IEntity;

public interface IEntity<TId> : IEntity, IEquatable<Entity<TId>> where TId : struct
{
    TId Id { get; }
}

public abstract class Entity<TId> : IEntity<TId> where TId : struct
{
    protected Entity()
    {
        Id = default;
    }

    protected Entity(TId id)
    {
        Id = id;
    }

    public TId Id { get; }

    public static bool operator ==(Entity<TId>? a, Entity<TId>? b)
    {
        if (a is null && b is null)
        {
            return true;
        }

        if (a is null || b is null)
        {
            return false;
        }

        return a.Equals(b);
    }

    public static bool operator !=(Entity<TId> a, Entity<TId> b) => !(a == b);

    /// <inheritdoc />
    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        return ReferenceEquals(this, other) || Id.Equals(other.Id);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return obj is Entity<TId> other && Id.Equals(other.Id);
    }

    /// <inheritdoc />
    public override int GetHashCode() => Id.GetHashCode() * 41;
}

namespace TheKittySaver.AdoptionSystem.Domain.Core.Primitives.BuildingBlocks;

public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>> where TId : struct
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Entity"/> class.
    /// </summary>
    /// <remarks>
    /// Required by EF Core.
    /// </remarks>
    protected Entity() : this(default)
    {
    }

    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public TId Id { get; } = id;

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
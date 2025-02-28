namespace PhoeNix.Domain.Primitives;

public abstract class Entity<TId>(TId id) : IEquatable<Entity<TId>>
{
    public TId Id { get; init; } = id;

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
            return false;

        if (other.GetType() != GetType())
            return false;

        return Id!.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Entity<TId>)obj);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
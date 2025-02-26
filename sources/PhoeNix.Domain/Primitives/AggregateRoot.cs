namespace PhoeNix.Domain.Primitives;

public abstract class AggregateRoot<TId> : Entity<TId> where TId : StronglyTypedId
{
    protected AggregateRoot(TId id) : base(id)
    {
    }
}
namespace PhoeNix.Domain.Primitives;

public abstract record StronglyTypedId(Guid Value)
{
    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator Guid(StronglyTypedId id)
    {
        return id.Value;
    }

    public static implicit operator string(StronglyTypedId id)
    {
        return id.Value.ToString();
    }
}
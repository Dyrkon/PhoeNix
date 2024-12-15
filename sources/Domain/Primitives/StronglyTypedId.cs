namespace Domain.Primitives;

public abstract record StronglyTypedId(Guid Value)
{
    public override string ToString()
    {
        return Value.ToString();
    }

    public static implicit operator string(StronglyTypedId stronglyTypedTypedId) =>
        stronglyTypedTypedId.Value.ToString();

    public static implicit operator Guid(StronglyTypedId stronglyTypedTypedId) => stronglyTypedTypedId.Value;
}

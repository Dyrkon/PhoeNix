namespace PhoeNix.Domain.Shared;

public sealed record Error(string Code, string? Description = null)
{
    public static readonly Error None = new(string.Empty);
    public static readonly Error NullValue = new(string.Empty);
    public static readonly Error ValueNotFound = new(string.Empty);

    public static implicit operator Result(Error error)
    {
        return Result.Failure(error);
    }

    public static implicit operator string(Error error)
    {
        return error.Code;
    }
}
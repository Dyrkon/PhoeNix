namespace Domain.Shared;

public record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);
    public static readonly Error NullValue = new("Error.NullValue", "The specific result value is null.");

    public static implicit operator string(Error error) => error.Code;
}

public record NotFoundError(string Code, string Message) : Error(Code, Message);
namespace PhoeNix.Domain.Shared;

public class Result
{
    protected internal Result(bool isSuccess, Error error)
    {
        if ((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
            throw new ArgumentException("Invalid error", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(true, Error.None, value);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result<T> Failure<T>(Error error)
    {
        return new Result<T>(false, error, default!);
    }

    public static implicit operator Task<Result>(Result value)
    {
        return Task.FromResult(value);
    }
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(bool isSuccess, Error error, T value) : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("Can't get value of failure.");

    public static implicit operator Result<T>(T? value)
    {
        return value is not null ? Success<T>(value) : Failure<T>(Error.NullValue);
    }

    public static implicit operator Task<Result<T>>(Result<T> value)
    {
        return Task.FromResult(value);
    }
}
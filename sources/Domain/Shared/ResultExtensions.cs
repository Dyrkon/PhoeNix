// https://www.youtube.com/watch?v=dDasAmowFts

namespace Domain.Shared;

public static class ResultExtensions
{
    public static async Task<Result<TIn>> Ensure<TIn>(this Result<TIn> result, Func<TIn, Task<bool>> predicate,
        Error error)
    {
        if (result.IsFailure) return result;

        return await predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, bool> predicate,
        Error error)
    {
        var result = await resultTask;
        if (result.IsFailure) return result;

        return predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    public static async Task<Result<TIn>> EnsureNotNull<TIn>(this Task<TIn?> objTask, Error? error = null)
    {
        var obj = await objTask;
        return obj is null ? Result.Failure<TIn>(error ?? Error.NullValue) : Result.Success(obj);
    }

    public static Result<TIn> EnsureNotNull<TIn>(this TIn? obj, Error? error = null)
    {
        return obj is null ? Result.Failure<TIn>(error ?? Error.NullValue) : Result.Success(obj);
    }

    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> bind)
    {
        var result = await resultTask;
        return result.IsSuccess ? bind(result.Value) : Result.Failure<TOut>(result.Error);
    }
}
using PhoeNix.Common.Models;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Extensions;

public static class ResultExtensions
{
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> bind)
    {
        return result.IsSuccess ? bind(result.Value) : Result.Failure<TOut>(result.Error);
    }

    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<Result<TOut>>> bind)
    {
        return result.IsSuccess ? await bind(result.Value) : Result.Failure<TOut>(result.Error);
    }

    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<TIn, Result<TOut>> bind)
    {
        var result = await resultTask;
        return result.IsSuccess ? bind(result.Value) : Result.Failure<TOut>(result.Error);
    }

    public static async Task<Result<TOut>> Bind<TIn, TOut>(this Task<Result<TIn>> resultTask,
        Func<TIn, Task<Result<TOut>>> bind)
    {
        var result = await resultTask;
        return result.IsSuccess ? await bind(result.Value) : Result.Failure<TOut>(result.Error);
    }

    public static Result Bind<TIn>(this Result<TIn> result, Func<TIn, Result> bind)
    {
        return result.IsSuccess ? bind(result.Value) : Result.Failure(result.Error);
    }

    public static async Task<Result> Bind<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> bind)
    {
        return result.IsSuccess ? await bind(result.Value) : Result.Failure(result.Error);
    }

    public static async Task<Result> Bind<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Result> bind)
    {
        var result = await resultTask;
        return result.IsSuccess ? bind(result.Value) : Result.Failure(result.Error);
    }

    public static async Task<Result> Bind<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result>> bind)
    {
        var result = await resultTask;
        return result.IsSuccess ? await bind(result.Value) : Result.Failure(result.Error);
    }

    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> map)
    {
        return result.IsSuccess ? Result.Success(map(result.Value)) : Result.Failure<TOut>(result.Error);
    }

    public static Result<TOut> Map<TOut>(this Result result, Func<TOut> map)
    {
        return result.IsSuccess ? Result.Success(map()) : Result.Failure<TOut>(result.Error);
    }

    public static async Task<Result<TOut>> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, Task<TOut>> map)
    {
        return result.IsSuccess ? Result.Success(await map(result.Value)) : Result.Failure<TOut>(result.Error);
    }

    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, TOut> map)
    {
        var result = await resultTask;
        return result.IsSuccess ? Result.Success(map(result.Value)) : Result.Failure<TOut>(result.Error);
    }

    public static async Task<Result<TOut>> Map<TIn, TOut>(this Task<Result<TIn>> resultTask, Func<TIn, Task<TOut>> map)
    {
        var result = await resultTask;
        return result.IsSuccess ? Result.Success(await map(result.Value)) : Result.Failure<TOut>(result.Error);
    }

    public static Result<TOut> TryCatch<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> func, Error error)
    {
        try
        {
            return result.IsSuccess ? Result.Success(func(result.Value)) : Result.Failure<TOut>(result.Error);
        }
        catch
        {
            return Result.Failure<TOut>(error);
        }
    }

    public static Result<TIn> Tap<TIn>(this Result<TIn> result, Action<TIn> action)
    {
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    public static Result Tap(this Result result, Action action)
    {
        if (result.IsSuccess) action();
        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(this Task<Result<TIn>> resultTask, Action<TIn> action)
    {
        var result = await resultTask;
        if (result.IsSuccess) action(result.Value);
        return result;
    }

    public static Result<TIn> Tap<TIn>(this Result<TIn> result, Func<TIn, Result> action)
    {
        if (result.IsSuccess)
        {
            var actionResult = action(result.Value);
            if (actionResult.IsFailure) return Result.Failure<TIn>(actionResult.Error);
        }

        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(this Result<TIn> result, Func<TIn, Task<Result>> action)
    {
        if (result.IsSuccess)
        {
            var actionResult = await action(result.Value);
            if (actionResult.IsFailure) return Result.Failure<TIn>(actionResult.Error);
        }

        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Result> action)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            var actionResult = action(result.Value);
            if (actionResult.IsFailure) return Result.Failure<TIn>(actionResult.Error);
        }

        return result;
    }

    public static async Task<Result<TIn>> Tap<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, Task<Result>> action)
    {
        var result = await resultTask;
        if (result.IsSuccess)
        {
            var actionResult = await action(result.Value);
            if (actionResult.IsFailure) return Result.Failure<TIn>(actionResult.Error);
        }

        return result;
    }

    public static Result<TIn> TapIfNotNull<TIn>(this Result<TIn> result, Func<TIn, Result> action)
    {
        if (result is { IsSuccess: true, Value: not null })
        {
            var actionResult = action(result.Value);
            if (actionResult.IsFailure) return Result.Failure<TIn>(actionResult.Error);
        }

        return result;
    }

    public static TOut Match<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> onSuccess, Func<Error, TOut> onFailure)
    {
        return result.IsSuccess ? onSuccess(result.Value) : onFailure(result.Error);
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

    public static async Task<Result<PagedResponse<TIn>>> EnsurePagedNotEmpty<TIn>(this Task<PagedResponse<TIn>> objTask,
        Error? error = null)
    {
        var obj = await objTask;
        return obj.Items is []
            ? Result.Failure<PagedResponse<TIn>>(error ?? Error.NullValue)
            : Result.Success(obj);
    }

    public static Result<TIn> Ensure<TIn>(this Result<TIn> result, Func<TIn, bool> predicate, Error error)
    {
        return result.IsFailure ? result : predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    public static async Task<Result<TIn>> Ensure<TIn>(this Result<TIn> result, Func<TIn, Task<bool>> predicate,
        Error error)
    {
        return result.IsFailure ? result : await predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, bool> predicate,
        Error error)
    {
        var result = await resultTask;
        return result.IsFailure ? result : predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask,
        Func<TIn, Task<bool>> predicate, Error error)
    {
        var result = await resultTask;
        return result.IsFailure ? result : await predicate(result.Value) ? result : Result.Failure<TIn>(error);
    }

    public static Result<TIn> Ensure<TIn>(this Result<TIn> result, Func<TIn, bool> predicate, Func<TIn, Error> error)
    {
        return result.IsFailure ? result : predicate(result.Value) ? result : Result.Failure<TIn>(error(result.Value));
    }

    public static async Task<Result<TIn>> Ensure<TIn>(this Result<TIn> result, Func<TIn, Task<bool>> predicate,
        Func<TIn, Error> error)
    {
        return result.IsFailure ? result :
            await predicate(result.Value) ? result : Result.Failure<TIn>(error(result.Value));
    }

    public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask, Func<TIn, bool> predicate,
        Func<TIn, Error> error)
    {
        var result = await resultTask;
        return result.Ensure(predicate, error);
    }

    public static async Task<Result<TIn>> Ensure<TIn>(this Task<Result<TIn>> resultTask,
        Func<TIn, Task<bool>> predicate, Func<TIn, Error> error)
    {
        var result = await resultTask;
        return await result.Ensure(predicate, error);
    }

    public static Result BindAll<TIn>(this IEnumerable<TIn> source, Func<TIn, Result> bind)
    {
        foreach (var item in source)
        {
            var r = bind(item);
            if (r.IsFailure) return r;
        }

        return Result.Success();
    }

    public static async Task<Result> Bind(this Task<Result> resultTask, Func<Result> bind)
    {
        var result = await resultTask;
        return result.IsFailure ? result : bind();
    }

    public static async Task<Result> Bind(this Task<Result> resultTask, Func<Task<Result>> bind)
    {
        var result = await resultTask;
        return result.IsFailure ? result : await bind();
    }
}
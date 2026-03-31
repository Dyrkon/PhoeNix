using PhoeNix.WebAPP.ApiClient.Contracts;

namespace PhoeNix.WebAPP.ApiClient.Models;

public sealed class ApiResult
{
    private ApiResult(bool isSuccess, ApiError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public ApiError? Error { get; }

    public static ApiResult Success()
    {
        return new ApiResult(true, null);
    }

    public static ApiResult Failure(ApiError error)
    {
        return new ApiResult(false, error);
    }
}

public sealed class ApiResult<T>
{
    private ApiResult(bool isSuccess, T? value, ApiError? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public ApiError? Error { get; }

    public static ApiResult<T> Success(T value)
    {
        return new ApiResult<T>(true, value, null);
    }

    public static ApiResult<T> Failure(ApiError error)
    {
        return new ApiResult<T>(false, default, error);
    }
}
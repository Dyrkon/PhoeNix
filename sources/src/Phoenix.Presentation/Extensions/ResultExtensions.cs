using Microsoft.AspNetCore.Http;
using PhoeNix.Domain.Shared;

namespace Phoenix.Presentation.Extensions;

public static class ResultExtensions
{
    public static IResult AsHttpResult(this Result result)
    {
        return result.IsFailure ? Results.BadRequest(result.Error) : Results.Ok();
    }

    public static IResult AsHttpResult<TValue>(this Result<TValue> result)
    {
        if (result.IsFailure)
            return Results.BadRequest(result.Error);

        return Results.Ok(result.Value);
    }
}
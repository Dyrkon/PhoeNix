using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Users.Commands;
using PhoeNix.Application.Users.Queries;
using PhoeNix.Contracts.Auth;
using PhoeNix.Domain.Shared;

namespace Phoenix.Presentation.Users;

public sealed class UsersModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", Register)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/me", Me)
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> Register(
        UserRegisterRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RegisterUserCommand(request.Name, request.Password), cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> Login(
        UserLoginRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LoginUserCommand(request.Name, request.Password), cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> Logout(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new LogoutUserCommand(), cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> Me(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCurrentUserQuery(), cancellationToken);
        return ToHttpResult(result);
    }

    private static IResult ToHttpResult(Result result)
    {
        if (result.IsSuccess)
            return Results.NoContent();

        return result.Error.Code switch
        {
            "UserUnauthenticated" => Results.Json(result.Error, statusCode: StatusCodes.Status401Unauthorized),
            _ => Results.BadRequest(result.Error)
        };
    }

    private static IResult ToHttpResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Value);

        return result.Error.Code switch
        {
            "UserNameAlreadyTaken" => Results.Conflict(result.Error),
            "UserInvalidCredentials" => Results.Json(result.Error, statusCode: StatusCodes.Status401Unauthorized),
            "UserUnauthenticated" => Results.Json(result.Error, statusCode: StatusCodes.Status401Unauthorized),
            "UserNotFound" => Results.NotFound(result.Error),
            _ => Results.BadRequest(result.Error)
        };
    }
}
using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Git;

namespace Phoenix.Presentation.GitOps;

public class GitOpsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/git-sync/push", TriggerPush)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapPost("/git-sync/pull", TriggerPull)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    private async Task<IResult> TriggerPush(
        IGitOpsPushOrchestrator orchestrator,
        ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Results.BadRequest(userIdResult.Error.Description);

        var result = await orchestrator.PushAsync(userIdResult.Value, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(result.Error.Description);
    }

    private async Task<IResult> TriggerPull(
        IGitOpsPullOrchestrator orchestrator,
        ICurrentUserAccessor currentUserAccessor,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Results.BadRequest(userIdResult.Error.Description);

        var result = await orchestrator.PullAsync(userIdResult.Value, cancellationToken);
        return result.IsSuccess
            ? Results.Ok()
            : Results.BadRequest(result.Error.Description);
    }
}

using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Provosioning.Commands;
using PhoeNix.Application.Provosioning.Queries;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Provisioning;

public sealed class ProvisioningModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/provisioning/machine/{machineId:guid}/start", StartProvisioning)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/v1/boot/{mac}", ProvideMachineBootDetails)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/provisioning/files/{sessionId:guid}/kernel", ProvideMachinePxeBootKernel)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/provisioning/files/{sessionId:guid}/init", ProvideMachinePxeBootRamDisk)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/provisioning/bootstrap/callback", RecordBootSignal)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> StartProvisioning(
        Guid machineId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new StartProvisioningSessionCommand(new MachineId(machineId));
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> ProvideMachineBootDetails(
        string mac,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new GetBootDecisionCommand(mac);
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> ProvideMachinePxeBootKernel(
        Guid sessionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProvisioningFiles(new ProvisioningSessionId(sessionId), BootFileType.Kernel);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.AsHttpResult();

        return Results.File(
            result.Value.Path,
            result.Value.ContentType,
            result.Value.DownloadName,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> ProvideMachinePxeBootRamDisk(
        Guid sessionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetProvisioningFiles(new ProvisioningSessionId(sessionId), BootFileType.RamDisk);
        var result = await sender.Send(query, cancellationToken);

        if (result.IsFailure)
            return result.AsHttpResult();

        return Results.File(
            result.Value.Path,
            result.Value.ContentType,
            result.Value.DownloadName,
            enableRangeProcessing: true);
    }

    private static async Task<IResult> RecordBootSignal(
        BootstrapCallbackRequest request,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Results.Unauthorized();

        var token = authorizationHeader["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
            return Results.Unauthorized();

        var command = new RecordBootSignalCommand(
            new ProvisioningSessionId(request.SessionId),
            new MachineId(request.MachineId),
            token);

        Console.WriteLine($"RECC");

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    public sealed record BootstrapCallbackRequest(Guid SessionId, Guid MachineId);
}
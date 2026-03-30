using System.Net;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Application.Setup.Queries;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Setup;

public sealed class SetupModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/setup/session/start", StartSetupSession)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/setup/session/{sessionId:guid}/machine/{machineId:guid}/start", StartMachineProvisioning)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/v1/boot/{mac}", ProvideMachineBootDetails)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/setup/files/{sessionId:guid}/kernel", ProvideMachinePxeBootKernel)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/setup/files/{sessionId:guid}/init", ProvideMachinePxeBootRamDisk)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/setup/bootstrap/callback", RecordBootSignal)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/setup/finalize", FinalizeMachineSetup)
            .AllowAnonymous()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/setup/session/{sessionId:guid}/machine/{machineId:guid}/status", GetMachineSetupStatus)
            .Produces<SetupStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/setup/session/{sessionId:guid}/cancel", CancelSetupSession)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> StartSetupSession(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new StartSetupSessionCommand(), cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> StartMachineProvisioning(
        Guid sessionId,
        Guid machineId,
        StartMachineSetupRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new StartMachineSetupCommand(
            new SetupSessionId(sessionId),
            new MachineId(machineId),
            new ConfigurationId(request.ConfigurationId),
            new SystemId(request.SystemId));

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> ProvideMachineBootDetails(
        string mac,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBootDecisionCommand(mac), cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> ProvideMachinePxeBootKernel(
        Guid sessionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSetupFiles(new SetupSessionId(sessionId), BootFileType.Kernel),
            cancellationToken);

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
        var result = await sender.Send(
            new GetSetupFiles(new SetupSessionId(sessionId), BootFileType.RamDisk),
            cancellationToken);

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

        var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null)
            return Results.BadRequest("Remote IP address could not be determined.");

        if (IPAddress.IsLoopback(remoteIpAddress))
            return Results.BadRequest("Loopback remote IP address is not valid for setup bootstrap callback.");

        var result = await sender.Send(
            new RecordBootSignalCommand(
                new SetupSessionId(request.SessionId),
                new MachineId(request.MachineId),
                token,
                remoteIpAddress),
            cancellationToken);

        return result.AsHttpResult();
    }

    private static async Task<IResult> FinalizeMachineSetup(
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

        var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null)
            return Results.BadRequest("Remote IP address could not be determined.");

        if (IPAddress.IsLoopback(remoteIpAddress))
            return Results.BadRequest("Loopback remote IP address is not valid for setup finalization callback.");

        var result = await sender.Send(
            new FinalizeMachineSetupCommand(token, remoteIpAddress),
            cancellationToken);

        return result.AsHttpResult();
    }

    private static async Task<IResult> GetMachineSetupStatus(
        Guid sessionId,
        Guid machineId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetSetupStatusQuery(
                new SetupSessionId(sessionId),
                new MachineId(machineId)),
            cancellationToken);

        return result.AsHttpResult();
    }

    private static async Task<IResult> CancelSetupSession(
        Guid sessionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new CancelSetupSessionCommand(new SetupSessionId(sessionId)),
            cancellationToken);

        return result.AsHttpResult();
    }
}
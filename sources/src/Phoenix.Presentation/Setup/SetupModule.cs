using System.Net;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Application.Setup.Queries;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
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

        app.MapPost("/setup/session/{sessionId:guid}/machine/{machineId:guid}/probe-hardware", ProbeMachineHardware)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/v1/boot/{mac}", ProvideMachineBootDetails)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/setup/files/{sessionId:guid}/kernel", ProvideMachinePxeBootKernel)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/setup/files/{sessionId:guid}/init", ProvideMachinePxeBootRamDisk)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/setup/bootstrap/callback", RecordBootSignal)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/setup/session/{sessionId:guid}/machine/{machineId:guid}/status", GetMachineSetupStatus)
            .Produces<SetupStage>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/setup/session/{sessionId:guid}/cancel", CancelSetupSession)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> StartSetupSession(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new StartSetupSessionCommand();
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> StartMachineProvisioning(
        Guid sessionId,
        Guid machineId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new StartMachineSetupCommand(
            new SetupSessionId(sessionId),
            new MachineId(machineId));

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> ProbeMachineHardware(
        Guid sessionId,
        Guid machineId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new GetMachineHardwareInformationCommand(
            new SetupSessionId(sessionId),
            new MachineId(machineId));

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
        var query = new GetSetupFiles(new SetupSessionId(sessionId), BootFileType.Kernel);
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
        var query = new GetSetupFiles(new SetupSessionId(sessionId), BootFileType.RamDisk);
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

        var remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is null)
            return Results.BadRequest("Remote IP address could not be determined.");

        if (IPAddress.IsLoopback(remoteIpAddress))
            return Results.BadRequest("Loopback remote IP address is not valid for setup bootstrap callback.");

        var command = new RecordBootSignalCommand(
            new SetupSessionId(request.SessionId),
            new MachineId(request.MachineId),
            token,
            remoteIpAddress);

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> GetMachineSetupStatus(
        Guid sessionId,
        Guid machineId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetSetupStatusQuery(
            new SetupSessionId(sessionId),
            new MachineId(machineId));

        var result = await sender.Send(query, cancellationToken);
        return result.AsHttpResult();
    }

    private static async Task<IResult> CancelSetupSession(
        Guid sessionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CancelSetupSessionCommand(new SetupSessionId(sessionId));
        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}
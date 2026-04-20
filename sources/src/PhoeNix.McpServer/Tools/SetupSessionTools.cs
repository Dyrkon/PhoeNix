using System.ComponentModel;
using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Setup.Commands;
using PhoeNix.Application.Setup.Queries;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.McpServer.Tools;

[McpServerToolType]
public static class SetupSessionTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [McpServerTool]
    [Description("""
        Start a new NixOS provisioning session. A session coordinates the installation of one or more machines.
        Returns the new session ID. After starting, use start_machine_provisioning to enroll machines into this session.
        """)]
    public static async Task<string> StartSetupSession(
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new StartSetupSessionCommand(), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(new { sessionId = result.Value }, JsonOptions);
    }

    [McpServerTool]
    [Description("List all provisioning sessions with summary status (total targets, done, failed). Most recent sessions first.")]
    public static async Task<string> ListSetupSessions(
        ISender sender,
        [Description("Page number (1-based)")] int page = 1,
        [Description("Number of items per page")] int pageSize = 15,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetSetupSessions(new SetupSessionsRequest(page, pageSize)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("""
        Get full details of a provisioning session including all machine targets.
        Each target shows: machineId, setupStage, IP address, selected configuration/system,
        disk assignments, and any errors. Use this to monitor provisioning progress.
        """)]
    public static async Task<string> GetSetupSession(
        ISender sender,
        [Description("Session ID (GUID)")] Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetSetupSessionDetail(new SetupSessionId(sessionId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("Get summary status of a provisioning session (total targets, done count, failed count, last transition time).")]
    public static async Task<string> GetSetupSessionStatus(
        ISender sender,
        [Description("Session ID (GUID)")] Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetSetupSessionStatus(new SetupSessionId(sessionId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("""
        Enroll a machine into a provisioning session and begin its setup workflow.
        The machine will PXE boot, get probed for hardware, then have NixOS installed
        using the specified configuration and system.
        The session must already be started (use start_setup_session first).
        """)]
    public static async Task StartMachineProvisioning(
        ISender sender,
        [Description("Session ID (GUID)")] Guid sessionId,
        [Description("Machine ID to provision (GUID)")] Guid machineId,
        [Description("Configuration ID to install (GUID)")] Guid configurationId,
        [Description("System ID within the configuration (GUID)")] Guid systemId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new StartMachineSetupCommand(
                new SetupSessionId(sessionId),
                new MachineId(machineId),
                new ConfigurationId(configurationId),
                new SystemId(systemId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);
    }

    [McpServerTool]
    [Description("""
        Get the current provisioning stage for a specific machine in a session.
        Setup stages: WaitingForPxe → ArtefactsAssigned → Bootstrapped → Probed → Orchestrated → Done.
        Also returns the last error if one occurred.
        """)]
    public static async Task<string> GetMachineSetupStatus(
        ISender sender,
        [Description("Session ID (GUID)")] Guid sessionId,
        [Description("Machine ID (GUID)")] Guid machineId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetSetupMachineStatusQuery(
                new SetupSessionId(sessionId),
                new MachineId(machineId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("Cancel an active provisioning session. All in-progress machine installations will be aborted.")]
    public static async Task CancelSetupSession(
        ISender sender,
        [Description("Session ID (GUID)")] Guid sessionId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new CancelSetupSessionCommand(new SetupSessionId(sessionId)),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);
    }
}

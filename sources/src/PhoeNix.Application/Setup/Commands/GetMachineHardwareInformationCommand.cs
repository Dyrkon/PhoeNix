using Microsoft.Extensions.Logging;
using PhoeNix.Application.Abstractions.HardwareProbing;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Setup;
using PhoeNix.Application.Setup.Extensions;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;
using ICommand = PhoeNix.Application.Abstractions.Messaging.ICommand;

namespace PhoeNix.Application.Setup.Commands;

public record GetMachineHardwareInformationCommand(
    SetupSessionId SessionId,
    MachineId MachineId) : ICommand;

internal sealed class GetMachineHardwareInformationCommandHandler(
    IHardwareProbeService hardwareProbeService,
    IHardwareInventoryProjector hardwareInventoryProjector,
    IInstallDiskSelectionPolicy installDiskSelectionPolicy,
    IMachineRepository machineRepository,
    ISetupSessionRepository sessionRepository,
    ILogger<GetMachineHardwareInformationCommandHandler> logger)
    : ICommandHandler<GetMachineHardwareInformationCommand>
{
    public async Task<Result> Handle(
        GetMachineHardwareInformationCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        var sessionResult = await sessionRepository
            .GetByIdAsync(request.SessionId, cancellationToken)
            .EnsureNotNull(new Error(
                "SetupSessionNotFound",
                $"Setup session '{request.SessionId.Value}' was not found."));

        if (sessionResult.IsFailure)
            return sessionResult.Error;

        var session = sessionResult.Value;

        var target = session.Targets.FirstOrDefault(t => t.MachineId == request.MachineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupTargetNotFound",
                $"Machine '{request.MachineId.Value}' is not enrolled in setup session '{request.SessionId.Value}'."));

        foreach (var disk in target.RankedDiskAssignments)
            logger.LogDebug(
                "Existing ranked disk assignment for machine {MachineId}: {Index} -> {DiskByIdPath}",
                request.MachineId.Value,
                disk.Index,
                disk.DiskByIdPath);

        if (target.Stage != SetupStage.Bootstrapped)
        {
            var error = new Error(
                "SetupTargetInvalidStage",
                $"Machine '{request.MachineId.Value}' must be in '{SetupStage.Bootstrapped}' stage before hardware probing.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(GetMachineHardwareInformationCommandHandler),
                nowUtc);
        }

        var machineResult = await machineRepository
            .GetByIdAsync(request.MachineId, cancellationToken)
            .EnsureNotNull(new Error(
                "MachineNotFound",
                $"Machine '{request.MachineId.Value}' was not found."));

        if (machineResult.IsFailure)
            return machineResult.Error;

        var machine = machineResult.Value;

        var probeResult = await hardwareProbeService.ProbeAsync(session, request.MachineId, cancellationToken);
        if (probeResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                probeResult.Error,
                nameof(GetMachineHardwareInformationCommandHandler),
                nowUtc);

        var hardwareProfileResult = hardwareInventoryProjector.Project(probeResult.Value);
        if (hardwareProfileResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                hardwareProfileResult.Error,
                nameof(GetMachineHardwareInformationCommandHandler),
                nowUtc);

        var recordHardwareResult = machine.RecordHardwareProfile(hardwareProfileResult.Value);
        if (recordHardwareResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                recordHardwareResult.Error,
                nameof(GetMachineHardwareInformationCommandHandler),
                nowUtc);

        if (machine.HardwareProfile is null)
        {
            var error = new Error(
                "MachineHardwareProfileMissing",
                "Machine hardware profile was not recorded.");

            return session.PersistFailure(
                request.MachineId,
                error,
                nameof(GetMachineHardwareInformationCommandHandler),
                nowUtc);
        }

        var rankedDisksResult = installDiskSelectionPolicy.Rank(
            machine.HardwareProfile.Disks,
            machine.InstallDiskSelectionPreference);

        if (rankedDisksResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                rankedDisksResult.Error,
                nameof(GetMachineHardwareInformationCommandHandler),
                nowUtc);

        var rankedDiskPaths = rankedDisksResult.Value
            .Select(d => d.StableDevicePath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToList();

        foreach (var rankedDiskPath in rankedDiskPaths)
            logger.LogDebug(
                "Ranked install disk candidate for machine {MachineId}: {DiskByIdPath}",
                request.MachineId.Value,
                rankedDiskPath);

        var assignRankedDisksResult = session.AssignRankedDisks(
            request.MachineId,
            rankedDiskPaths);

        if (assignRankedDisksResult.IsFailure)
            return session.PersistFailure(
                request.MachineId,
                assignRankedDisksResult.Error,
                nameof(GetMachineHardwareInformationCommandHandler),
                nowUtc);

        return session.UpdateMachineStage(
            request.MachineId,
            SetupStage.Probed,
            nowUtc);
    }
}
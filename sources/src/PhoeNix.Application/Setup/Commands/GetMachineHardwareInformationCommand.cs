using PhoeNix.Application.Abstractions.HardwareProbing;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Abstractions.Setup;
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

internal sealed record GetMachineHardwareInformationCommandHandler(
    IHardwareProbeService HardwareProbeService,
    IHardwareInventoryProjector HardwareInventoryProjector,
    IInstallDiskSelectionPolicy InstallDiskSelectionPolicy,
    IMachineRepository MachineRepository,
    ISetupSessionRepository SessionRepository)
    : ICommandHandler<GetMachineHardwareInformationCommand>
{
    public async Task<Result> Handle(
        GetMachineHardwareInformationCommand request,
        CancellationToken cancellationToken)
    {
        var sessionResult = await SessionRepository.GetByIdAsync(request.SessionId, cancellationToken).EnsureNotNull(
            new Error(
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
            Console.WriteLine($"Orchestration disk: {disk.Index} -> {disk.DiskByIdPath}");

        if (target.Stage != SetupStage.Bootstrapped)
            return Result.Failure(new Error(
                "SetupTargetInvalidStage",
                $"Machine '{request.MachineId.Value}' must be in '{SetupStage.Bootstrapped}' stage before hardware probing."));

        var machineResult = await MachineRepository.GetByIdAsync(request.MachineId, cancellationToken).EnsureNotNull(
            new Error(
                "MachineNotFound",
                $"Machine '{request.MachineId.Value}' was not found."));
        if (machineResult.IsFailure)
            return machineResult.Error;

        var machine = machineResult.Value;

        var probeResult = await HardwareProbeService.ProbeAsync(session, request.MachineId, cancellationToken);
        if (probeResult.IsFailure)
            return probeResult.Error;

        var hardwareProfileResult = HardwareInventoryProjector.Project(probeResult.Value);
        if (hardwareProfileResult.IsFailure)
            return hardwareProfileResult.Error;

        var recordHardwareResult = machine.RecordHardwareProfile(hardwareProfileResult.Value);
        if (recordHardwareResult.IsFailure)
            return recordHardwareResult.Error;

        if (machine.HardwareProfile is null)
            return Result.Failure(new Error(
                "MachineHardwareProfileMissing",
                "Machine hardware profile was not recorded."));

        var rankedDisksResult = InstallDiskSelectionPolicy.Rank(
            machine.HardwareProfile.Disks,
            machine.InstallDiskSelectionPreference);

        if (rankedDisksResult.IsFailure)
            return rankedDisksResult.Error;

        var rankedDiskPaths = rankedDisksResult.Value
            .Select(d => d.StableDevicePath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .ToList();

        foreach (var rankedDiskPath in rankedDiskPaths) Console.WriteLine($"Path1: {rankedDiskPath}");

        var assignRankedDisksResult = session.AssignRankedDisks(
            request.MachineId,
            rankedDiskPaths);

        if (assignRankedDisksResult.IsFailure)
            return assignRankedDisksResult.Error;

        return session.UpdateMachineStage(request.MachineId, SetupStage.Probed);
    }
}
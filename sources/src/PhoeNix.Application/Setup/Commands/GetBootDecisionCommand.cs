using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Options;
using PhoeNix.Application.Repositories;
using PhoeNix.Application.Setup;
using PhoeNix.Application.Setup.Extensions;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Setup.Commands;

public record GetBootDecisionCommand(string MacAddress) : ICommand<PxeBootDetails>;

internal sealed class GetBootDecisionQueryHandler(
    IMachineRepository machineRepository,
    ISetupSessionRepository setupSessionRepository,
    IOptions<NetbootHostOptions> netbootOptions)
    : ICommandHandler<GetBootDecisionCommand, PxeBootDetails>
{
    public async Task<Result<PxeBootDetails>> Handle(
        GetBootDecisionCommand request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;

        if (!PhysicalAddress.TryParse(request.MacAddress, out var address))
            return Result.Failure<PxeBootDetails>(new Error(
                "MachineMacInvalid",
                $"Unable to parse machine MAC address '{request.MacAddress}'."));

        var machineResult = await machineRepository
            .GetByMacAddressAsync(address, cancellationToken)
            .EnsureNotNull(new Error(
                "MachineNotFound",
                "Machine with the provided MAC address was not found."));

        if (machineResult.IsFailure)
            return Result.Failure<PxeBootDetails>(machineResult.Error);

        var machine = machineResult.Value;

        var sessionResult = await setupSessionRepository
            .GetWithEnrolledMachineAsync(machine.Id, cancellationToken)
            .EnsureNotNull(new Error(
                "SetupSessionNotFound",
                "No active setup session was found for the machine."));

        if (sessionResult.IsFailure)
            return Result.Failure<PxeBootDetails>(sessionResult.Error);

        var session = sessionResult.Value;

        if (session.BootArtefactDescriptor is null)
        {
            var error = new Error(
                "SetupSessionBootArtefactMissing",
                "The setup session does not have a boot artefact assigned.");

            return session.PersistFailure<PxeBootDetails>(
                machine.Id,
                error,
                nameof(GetBootDecisionQueryHandler),
                nowUtc);
        }

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machine.Id);
        if (target is null)
            return Result.Failure<PxeBootDetails>(new Error(
                "SetupTargetNotFound",
                "Setup target was not found for the machine."));

        if (target.Stage == SetupStage.Bootstrapped) return Result.Success<PxeBootDetails>(null!);

        if (target.Stage is not (SetupStage.WaitingForPxe or SetupStage.ArtefactsAssigned))
        {
            var error = new Error(
                "SetupTargetInvalidStage",
                $"Machine '{machine.Id.Value}' must be in '{SetupStage.WaitingForPxe}' or '{SetupStage.ArtefactsAssigned}' stage before boot details can be provided.");

            return session.PersistFailure<PxeBootDetails>(
                machine.Id,
                error,
                nameof(GetBootDecisionQueryHandler),
                nowUtc);
        }

        if (target.CallbackToken is null)
        {
            var error = new Error(
                "SetupCallbackTokenMissing",
                "No callback token is assigned to the setup target.");

            return session.PersistFailure<PxeBootDetails>(
                machine.Id,
                error,
                nameof(GetBootDecisionQueryHandler),
                nowUtc);
        }

        if (!target.CallbackToken.IsValid(nowUtc))
        {
            var error = new Error(
                "SetupCallbackTokenInvalid",
                "The callback token is expired or revoked.");

            return session.PersistFailure<PxeBootDetails>(
                machine.Id,
                error,
                nameof(GetBootDecisionQueryHandler),
                nowUtc);
        }

        if (target.Stage == SetupStage.WaitingForPxe)
        {
            var stageResult = session.UpdateMachineStage(
                machine.Id,
                SetupStage.ArtefactsAssigned,
                nowUtc);

            if (stageResult.IsFailure)
                return Result.Failure<PxeBootDetails>(stageResult.Error);
        }

        var cmdline = BuildCommandLine(
            session.BootArtefactDescriptor,
            session.Id,
            machine.Id,
            netbootOptions.Value.ApiBasePublicUrl,
            target.CallbackToken.Token);

        return Result.Success(new PxeBootDetails(
            $"/api/setup/files/{session.Id.Value:D}/kernel",
            [$"/api/setup/files/{session.Id.Value:D}/init"],
            cmdline,
            $"Booting {machine.Title}"));
    }

    private static string BuildCommandLine(
        BootstrapImageDescriptor image,
        SetupSessionId sessionId,
        MachineId machineId,
        string apiBasePath,
        string callbackToken)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(image.Init))
            parts.Add($"init={image.Init}");

        parts.Add("loglevel=4");
        parts.Add($"phoenix.session-id={sessionId.Value:D}");
        parts.Add($"phoenix.machine-id={machineId.Value:D}");
        parts.Add($"phoenix.callback-token={callbackToken}");
        parts.Add($"phoenix.api-base={apiBasePath}");

        return string.Join(" ", parts);
    }
}
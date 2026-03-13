using System.Net.NetworkInformation;
using Microsoft.Extensions.Options;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Models.Bootstrap;
using PhoeNix.Application.Options;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Provosioning.Commands;

public record GetBootDecisionCommand(string MacAddress) : ICommand<PxeBootDetails>;

internal sealed class GetBootDecisionQueryHandler(
    IMachineRepository machineRepository,
    IProvisioningSessionRepository provisioningSessionRepository,
    IOptions<NetbootHostOptions> netbootOptions)
    : ICommandHandler<GetBootDecisionCommand, PxeBootDetails>
{
    public async Task<Result<PxeBootDetails>> Handle(
        GetBootDecisionCommand request,
        CancellationToken cancellationToken)
    {
        if (!PhysicalAddress.TryParse(request.MacAddress, out var address))
            return Result.Failure<PxeBootDetails>(new Error("MachineMACError",
                $"Unable to parse machine MAC address {request.MacAddress}"));

        var machineResult = await machineRepository
            .GetByMacAddressAsync(address, cancellationToken)
            .EnsureNotNull(new Error("MachineNotFound", "Machine with the provided MAC address was not found."));

        if (machineResult.IsFailure)
            return Result.Failure<PxeBootDetails>(machineResult.Error);

        var machine = machineResult.Value;

        var sessionResult = await provisioningSessionRepository
            .GetWithEnrolledMachineAsync(machine.Id, cancellationToken)
            .EnsureNotNull(new Error("ProvisioningSessionNotFound",
                "No active provisioning session was found for the machine."));

        if (sessionResult.IsFailure)
            return Result.Failure<PxeBootDetails>(sessionResult.Error);

        var session = sessionResult.Value;

        if (session.BootArtefactDescriptor is null)
            return Result.Failure<PxeBootDetails>(new Error("SessionNoBootArtefact"));

        var target = session.Targets.FirstOrDefault(t => t.MachineId == machine.Id);
        if (target is null)
            return Result.Failure<PxeBootDetails>(new Error(
                "ProvisioningTargetNotFound",
                "Provisioning target was not found for the machine."));

        if (target.Stage != ProvisioningStage.WaitingForPxe &&
            target.Stage != ProvisioningStage.SecretsGenerated)
            return Result.Failure<PxeBootDetails>(new Error(
                "ProvisioningTargetNotBootable",
                $"Machine is not in a bootable stage. Current stage: {target.Stage}."));


        var cmdline = BuildCommandLine(session.BootArtefactDescriptor!, session.Id, machine.Id,
            netbootOptions.Value.ApiBasePublicUrl,
            target.CallbackToken?.Token ?? "");

        session.UpdateMachineStage(machine.Id, ProvisioningStage.WaitingForPxe);

        return Result.Success(new PxeBootDetails(
            $"/provisioning/files/{session.Id.Value:D}/kernel",
            [$"/provisioning/files/{session.Id.Value:D}/init"],
            cmdline,
            $"Booting {machine.Title}"));
    }

    private static string BuildCommandLine(
        BootstrapImageDescriptor image,
        ProvisioningSessionId sessionId,
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
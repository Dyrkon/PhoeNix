using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Models.Authentication;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.ProvisioningSessions;

public class ProvisioningSession : AggregateRoot<ProvisioningSessionId>
{
    private readonly List<ProvisioningTarget> _targets = [];

    private ProvisioningSession(ProvisioningSessionId id) : base(id)
    {
    }

    public BootArtefactDescriptor? BootArtefactDescriptor { get; private set; }

    public SshCredential? SshCredential { get; private set; }

    public IReadOnlyCollection<ProvisioningTarget> Targets => _targets.AsReadOnly();

    public bool ReadyForProvisioning(DateTime nowUtc)
    {
        return BootArtefactDescriptor is not null
               && SshCredential is not null
               && SshCredential.IsValid(nowUtc)
               && _targets.Count > 0
               && _targets.All(t => t.CallbackToken.IsValid(nowUtc));
    }

    public Result AssignSshCredential(SshCredential credential, DateTime nowUtc)
    {
        if (!credential.IsValid(nowUtc))
            return Result.Failure(new Error(
                "ProvisioningSessionSshCredentialInvalid",
                "SSH credential is expired or revoked."
            ));

        SshCredential = credential;
        return Result.Success();
    }

    public Result RevokeSshCredential(DateTime nowUtc)
    {
        if (SshCredential is null)
            return Result.Failure(new Error(
                "ProvisioningSessionSshCredentialMissing",
                "No SSH credential is assigned to this provisioning session."
            ));

        if (SshCredential.RevokedAtUtc is not null)
            return Result.Failure(new Error(
                "ProvisioningSessionSshCredentialAlreadyRevoked",
                "SSH credential is already revoked."
            ));

        SshCredential = SshCredential with { RevokedAtUtc = nowUtc };
        return Result.Success();
    }

    public Result AssignBootstrapArtefact(string kernelLocation, string initRdLocation, string cmdLine)
    {
        if (kernelLocation.First() != '/')
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                "Kernel location has to be absolute path."));
        if (initRdLocation.First() != '/')
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                "Ramdisk location has to be absolute path."));
        if (kernelLocation.Split('/').ToList().Slice(1, 2) is not ["nix", "store"])
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                "Kernel location has to be a store path."));
        if (initRdLocation.Split('/').ToList().Slice(1, 2) is not ["nix", "store"])
            return Result.Failure(new Error("ProvisioningSessionIncorrectArtefact",
                "Ramdisk location has to be a store path."));

        BootArtefactDescriptor = new BootArtefactDescriptor(kernelLocation, initRdLocation, cmdLine);
        return Result.Success();
    }

    public Result EnrollMachine(MachineId machineId, CallbackToken callbackToken, DateTime nowUtc)
    {
        if (!callbackToken.IsValid(nowUtc))
            return Result.Failure(new Error(
                "ProvisioningSessionCallbackTokenInvalid",
                "Callback token is expired or revoked."
            ));

        if (_targets.Any(t => t.MachineId == machineId))
            return Result.Failure(new Error(
                "ProvisioningSessionMachineAlreadyEnrolled",
                $"Machine '{machineId.Value}' is already enrolled in this provisioning session."
            ));

        return ProvisioningTarget.Create(machineId, callbackToken).Tap(machine => _targets.Add(machine));
    }

    public Result RevokeMachineCallbackToken(MachineId machineId, DateTime nowUtc)
    {
        var index = _targets.FindIndex(t => t.MachineId == machineId);
        if (index < 0)
            return Result.Failure(new Error(
                "ProvisioningSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this provisioning session."
            ));

        var target = _targets[index];

        if (target.CallbackToken.RevokedAtUtc is not null)
            return Result.Failure(new Error(
                "ProvisioningSessionCallbackTokenAlreadyRevoked",
                $"Callback token for machine '{machineId.Value}' is already revoked."
            ));

        _targets[index] = target
            with
            {
                CallbackToken = target.CallbackToken with { RevokedAtUtc = nowUtc }
            };

        return Result.Success();
    }

    public Result UpdateMachineStage(MachineId machineId, ProvisioningStage stage)
    {
        var index = _targets.FindIndex(t => t.MachineId == machineId);
        if (index < 0)
            return Result.Failure(new Error(
                "ProvisioningSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this provisioning session."
            ));

        _targets[index] = _targets[index].WithStage(stage);
        return Result.Success();
    }

    public static Result<ProvisioningSession> Create(ProvisioningSessionId id)
    {
        return Result.Success(new ProvisioningSession(id));
    }
}
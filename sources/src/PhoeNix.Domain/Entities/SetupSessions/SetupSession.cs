using System.Net;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Events;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.SetupSessions;

public class SetupSession : AggregateRoot<SetupSessionId>
{
    private readonly List<SetupTarget> _targets = [];

    private SetupSession(SetupSessionId id) : base(id)
    {
    }

    public BootstrapImageDescriptor? BootArtefactDescriptor { get; private set; }

    public SshCredential? SshCredential { get; private set; }

    public IReadOnlyCollection<SetupTarget> Targets => _targets;

    public DateTime StartTime { get; private set; }

    public Result AssignSshCredential(SshCredential credential, DateTime nowUtc)
    {
        if (!credential.IsValid(nowUtc))
            return Result.Failure(new Error(
                "SetupSessionSshCredentialInvalid",
                "SSH credential is expired or revoked."));

        SshCredential = credential;
        return Result.Success();
    }

    public Result RevokeSshCredential(DateTime nowUtc)
    {
        if (SshCredential is null)
            return Result.Failure(new Error(
                "SetupSessionSshCredentialMissing",
                "No SSH credential is assigned to this setup session."));

        if (SshCredential.RevokedAtUtc is not null)
            return Result.Failure(new Error(
                "SetupSessionSshCredentialAlreadyRevoked",
                "SSH credential is already revoked."));

        SshCredential = SshCredential with { RevokedAtUtc = nowUtc };
        return Result.Success();
    }

    public Result AssignBootstrapArtefact(string kernelLocation, string initRdLocation, string cmdLine)
    {
        if (kernelLocation.First() != '/')
            return Result.Failure(new Error(
                "SetupSessionIncorrectArtefact",
                "Kernel location has to be absolute path."));

        if (initRdLocation.First() != '/')
            return Result.Failure(new Error(
                "SetupSessionIncorrectArtefact",
                "Ramdisk location has to be absolute path."));

        if (kernelLocation.Split('/').ToList().Slice(1, 2) is not ["nix", "store"])
            return Result.Failure(new Error(
                "SetupSessionIncorrectArtefact",
                "Kernel location has to be a store path."));

        if (initRdLocation.Split('/').ToList().Slice(1, 2) is not ["nix", "store"])
            return Result.Failure(new Error(
                "SetupSessionIncorrectArtefact",
                "Ramdisk location has to be a store path."));

        BootArtefactDescriptor = new BootstrapImageDescriptor(kernelLocation, initRdLocation, cmdLine);
        return Result.Success();
    }

    public Result EnrollMachine(
        MachineId machineId,
        SystemId systemId,
        ConfigurationId configurationId,
        DateTime nowUtc)
    {
        if (_targets.Any(t => t.MachineId == machineId))
            return Result.Failure(new Error(
                "SetupSessionMachineAlreadyEnrolled",
                $"Machine '{machineId.Value}' is already enrolled in this setup session."));

        return SetupTarget
            .Create(machineId, systemId, configurationId, nowUtc)
            .Tap(target => _targets.Add(target));
    }

    public Result UpdateMachineStage(
        MachineId machineId,
        SetupStage stage,
        DateTime nowUtc,
        bool clearFailure = true)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        var previousStage = target.Stage;

        var result = target.SetStage(stage, nowUtc, clearFailure);
        if (result.IsFailure)
            return result;

        if (previousStage != stage)
            RaiseDomainEvent(new SetupTargetStageChangedDomainEvent(
                Id,
                machineId,
                previousStage,
                stage));

        return Result.Success();
    }

    public Result RecordMachineFailure(
        MachineId machineId,
        Error error,
        string source,
        DateTime nowUtc)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        return target.RecordFailure(error, source, nowUtc);
    }

    public Result ClearMachineFailure(MachineId machineId)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        return target.ClearFailure();
    }

    public Result AssignMachineCallbackToken(MachineId id, CallbackToken callbackToken)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == id);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{id.Value}' is not enrolled in this setup session."));

        return target.AssignToken(callbackToken);
    }

    public Result ClearCallbackToken(MachineId machineId)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        return target.ClearCallbackToken();
    }

    public Result RevokeMachineCallbackToken(MachineId machineId, DateTime nowUtc)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        if (target.CallbackToken?.RevokedAtUtc is not null)
            return Result.Failure(new Error(
                "SetupSessionCallbackTokenAlreadyRevoked",
                $"Callback token for machine '{machineId.Value}' is already revoked."));

        return target.RevokeCallbackToken(nowUtc);
    }

    public Result RecordMachineIpAddress(MachineId machineId, IPAddress ipAddress)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        return target.SetIpAddress(ipAddress);
    }

    public Result AssignRankedDisks(MachineId machineId, IReadOnlyList<string> diskByIdPaths)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        return target.AssignRankedDisks(diskByIdPaths);
    }

    public Result ClearRankedDisks(MachineId machineId)
    {
        var target = _targets.FirstOrDefault(t => t.MachineId == machineId);
        if (target is null)
            return Result.Failure(new Error(
                "SetupSessionMachineNotEnrolled",
                $"Machine '{machineId.Value}' is not enrolled in this setup session."));

        return target.ClearRankedDisks();
    }

    public static Result<SetupSession> Create(SetupSessionId id, DateTime now)
    {
        return Result.Success(new SetupSession(id) { StartTime = now });
    }
}
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Primitives;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.Machines;

public class Machine : AggregateRoot<MachineId>
{
    private static readonly Regex HostnameRegex =
        new(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$", RegexOptions.Compiled);

    public static bool IsValidHostname(string hostname) =>
        !string.IsNullOrWhiteSpace(hostname) &&
        hostname.Length <= 63 &&
        HostnameRegex.IsMatch(hostname);
    private Machine(MachineId id) : base(id)
    {
    }

    public UserId OwnerId { get; private set; } = default!;

    public string Title { get; private set; }

    public bool Enabled { get; private set; }

    public PhysicalAddress MacAddress { get; private set; }

    public Architecture Architecture { get; private set; }

    public InstallDiskSelectionPreference InstallDiskSelectionPreference { get; private set; }

    public HardwareProfile? HardwareProfile { get; private set; }

    public SoftwareSnapshot? SoftwareSnapshot { get; private set; }

    public DeploymentSnapshot? DeploymentSnapshot { get; private set; }

    public MachineStatus MachineStatus { get; private set; }

    public Result ChangeTitle(string newTitle, DateTime now)
    {
        if (!IsValidHostname(newTitle))
            return Result.Failure(new Error(
                "MachineTitleInvalidHostname",
                $"Machine title '{newTitle}' is not a valid RFC 1123 hostname. Use lowercase letters, digits, and hyphens only (no leading/trailing hyphens, max 63 chars)."));

        Title = newTitle;

        if (MachineStatus.MachineState is MachineState.Orchestrated or MachineState.Updated or MachineState.OutDated)
            ChangeMachineState(MachineState.OutDated, now);

        return Result.Success();
    }

    public Result ChangeMacAddress(string addressString)
    {
        if (!PhysicalAddress.TryParse(addressString, out var address))
            return Result.Failure(new Error(
                "MachineMacAddressInvalid",
                $"Unable to parse machine MAC address '{addressString}'."));

        MacAddress = address;
        return Result.Success();
    }

    public Result ChangeArchitecture(Architecture architecture)
    {
        Architecture = architecture;
        return Result.Success();
    }

    public Result ChangeInstallDiskSelectionPreference(InstallDiskSelectionPreference preference)
    {
        InstallDiskSelectionPreference = preference;
        return Result.Success();
    }

    public Result RecordHardwareProfile(HardwareProfile hardwareProfile)
    {
        HardwareProfile = hardwareProfile;
        return Result.Success();
    }

    public Result ClearHardwareProfile()
    {
        HardwareProfile = null;
        return Result.Success();
    }

    public Result RecordDeploymentSnapshot(
        ConfigurationId configurationId,
        string configurationTitle,
        SystemId systemId,
        string systemName,
        IPAddress ipAddress,
        DateTime nowUtc,
        IReadOnlyList<string> boundDiskPaths)
    {
        var snapshotResult = DeploymentSnapshot.Create(
            configurationId,
            configurationTitle,
            systemId,
            systemName,
            ipAddress,
            nowUtc,
            boundDiskPaths);

        if (snapshotResult.IsFailure)
            return snapshotResult.Error;

        DeploymentSnapshot = snapshotResult.Value;
        return Result.Success();
    }

    public Result Enable()
    {
        if (Enabled)
            return Result.Failure(new Error(
                "MachineAlreadyEnabled",
                $"Machine '{Title}' is already enabled."));

        Enabled = true;
        return Result.Success();
    }

    public Result Disable()
    {
        if (!Enabled)
            return Result.Failure(new Error(
                "MachineAlreadyDisabled",
                $"Machine '{Title}' is already disabled."));

        Enabled = false;
        return Result.Success();
    }

    public Result ChangeMachineState(MachineState machineState, DateTime now)
    {
        return MachineStatus.ChangeMachineState(machineState, now);
    }

    public static Result<Machine> Create(
        MachineId machineId,
        UserId ownerId,
        string macAddress,
        string title,
        bool enabled,
        Architecture architecture,
        InstallDiskSelectionPreference installDiskSelectionPreference)
    {
        if (!IsValidHostname(title))
            return Result.Failure<Machine>(new Error(
                "MachineTitleInvalidHostname",
                $"Machine title '{title}' is not a valid RFC 1123 hostname. Use lowercase letters, digits, and hyphens only (no leading/trailing hyphens, max 63 chars)."));

        return Result.Success(new Machine(machineId)
                {
                    OwnerId = ownerId,
                    Title = title,
                    Enabled = enabled,
                    Architecture = architecture,
                    InstallDiskSelectionPreference = installDiskSelectionPreference,
                    MachineStatus = new MachineStatus(MachineState.Registered)
                })
            .Tap(machine => machine.ChangeMacAddress(macAddress));
    }
}
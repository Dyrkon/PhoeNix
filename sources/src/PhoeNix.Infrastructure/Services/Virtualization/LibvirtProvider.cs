using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using PhoeNix.Application.Abstractions.Processes;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Virtualization;

public sealed class LibvirtProvider(IProcessRunner processRunner) : IVirtualizationProvider
{
    public VmHostProvider ProviderType => VmHostProvider.Libvirt;

    public Task<Result> TestConnectionAsync(VmHostCredential credential, CancellationToken ct)
    {
        var uri = BuildConnectionUri(credential);
        var result = processRunner.RunProcess("virsh", ["-c", uri, "version"], ct);

        return Task.FromResult(result.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error("Libvirt.ConnectionFailed",
                $"Failed to connect to libvirt host: {result.Error.Description}")));
    }

    public Task<Result<VmHostResources>> GetResourcesAsync(VmHostCredential credential, CancellationToken ct)
    {
        var uri = BuildConnectionUri(credential);
        var nodeInfoResult = processRunner.RunProcess("virsh", ["-c", uri, "nodeinfo"], ct);
        if (nodeInfoResult.IsFailure)
            return Task.FromResult(Result.Failure<VmHostResources>(nodeInfoResult.Error));

        var output = nodeInfoResult.Value.StandardOutput;
        var cpuCores = ParseNodeInfoValue(output, "CPU(s)");
        var memoryKb = ParseNodeInfoLongValue(output, "Memory size");

        var extraConfig = ParseExtraConfig(credential.ExtraConfig);
        var storagePool = extraConfig.GetValueOrDefault("storagePool", "default");

        long totalStorageGb = 0;
        long usedStorageGb = 0;
        var poolInfoResult = processRunner.RunProcess("virsh", ["-c", uri, "pool-info", storagePool], ct);
        if (poolInfoResult.IsSuccess)
        {
            var poolOutput = poolInfoResult.Value.StandardOutput;
            var capacityGb = ParsePoolSizeGb(poolOutput, "Capacity");
            var allocationGb = ParsePoolSizeGb(poolOutput, "Allocation");
            totalStorageGb = (long)capacityGb;
            usedStorageGb = (long)allocationGb;
        }

        var resources = VmHostResources.Create(
            cpuCores, 0,
            memoryKb / 1024, 0,
            totalStorageGb, usedStorageGb);

        return Task.FromResult(Result.Success(resources));
    }

    public Task<Result<CreatedVmInfo>> CreateVmAsync(
        VmHostCredential credential, VmDefinition definition, CancellationToken ct)
    {
        var uri = BuildConnectionUri(credential);
        var extraConfig = ParseExtraConfig(credential.ExtraConfig);
        var storagePool = extraConfig.GetValueOrDefault("storagePool", "default");
        var networkName = definition.NetworkBridge ?? extraConfig.GetValueOrDefault("network", "default");

        var args = new List<string>
        {
            "--connect", uri,
            "--name", definition.Name,
            "--memory", definition.MemoryMb.ToString(),
            "--vcpus", definition.CpuCores.ToString(),
            "--cpu", "host-passthrough",
            "--controller", "type=scsi,model=virtio-scsi",
            "--disk", $"pool={storagePool},size={definition.DiskSizeGb},bus=scsi",
            "--network", $"network={networkName},model=e1000e",
            "--boot", "uefi,hd,network",
            "--os-variant", "fedora-unknown",
            "--graphics", "spice,listen=0.0.0.0",
            "--video", "qxl",
            "--noautoconsole"
        };

        var result = processRunner.RunProcess("virt-install", args, ct);
        if (result.IsFailure)
            return Task.FromResult(Result.Failure<CreatedVmInfo>(new Error(
                "Libvirt.CreateVmFailed",
                $"Failed to create VM: {result.Error.Description}")));

        var macResult = processRunner.RunProcess("virsh",
            ["-c", uri, "domiflist", definition.Name, "--all"], ct);

        var macAddress = "00:00:00:00:00:00";
        if (macResult.IsSuccess)
        {
            var macMatch = Regex.Match(macResult.Value.StandardOutput,
                @"([0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2})");
            if (macMatch.Success)
                macAddress = macMatch.Value;
        }

        return Task.FromResult(Result.Success(
            new CreatedVmInfo(definition.Name, definition.Name, macAddress)));
    }

    public Task<Result> DeleteVmAsync(VmHostCredential credential, string externalId, CancellationToken ct)
    {
        var uri = BuildConnectionUri(credential);

        processRunner.RunProcess("virsh", ["-c", uri, "destroy", externalId], ct);

        var undefineResult = processRunner.RunProcess("virsh",
            ["-c", uri, "undefine", externalId, "--remove-all-storage", "--nvram"], ct);

        if (undefineResult.IsFailure)
        {
            undefineResult = processRunner.RunProcess("virsh",
                ["-c", uri, "undefine", externalId, "--remove-all-storage"], ct);
        }

        return Task.FromResult(undefineResult.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error("Libvirt.DeleteVmFailed",
                $"Failed to delete VM: {undefineResult.Error.Description}")));
    }

    public Task<Result> PowerActionAsync(
        VmHostCredential credential, string externalId, PowerAction action, CancellationToken ct)
    {
        var uri = BuildConnectionUri(credential);
        var command = action switch
        {
            PowerAction.Start => "start",
            PowerAction.Stop => "shutdown",
            PowerAction.ForceStop => "destroy",
            PowerAction.Reboot => "reboot",
            PowerAction.ForceReboot => "reset",
            _ => throw new ArgumentOutOfRangeException(nameof(action))
        };

        var result = processRunner.RunProcess("virsh", ["-c", uri, command, externalId], ct);

        return Task.FromResult(result.IsSuccess
            ? Result.Success()
            : Result.Failure(new Error("Libvirt.PowerActionFailed",
                $"Power action '{action}' failed: {result.Error.Description}")));
    }

    public Task<Result<VmPowerState>> GetPowerStateAsync(
        VmHostCredential credential, string externalId, CancellationToken ct)
    {
        var uri = BuildConnectionUri(credential);
        var result = processRunner.RunProcess("virsh", ["-c", uri, "domstate", externalId], ct);

        if (result.IsFailure)
            return Task.FromResult(Result.Success(VmPowerState.Unknown));

        var state = result.Value.StandardOutput.Trim().ToLower() switch
        {
            "running" => VmPowerState.Running,
            "shut off" => VmPowerState.Stopped,
            "paused" => VmPowerState.Paused,
            _ => VmPowerState.Unknown
        };

        return Task.FromResult(Result.Success(state));
    }

    public Task<Result<IReadOnlyList<DiscoveredVm>>> ListVmsAsync(
        VmHostCredential credential, CancellationToken ct)
    {
        var uri = BuildConnectionUri(credential);
        var listResult = processRunner.RunProcess("virsh",
            ["-c", uri, "list", "--all", "--name"], ct);

        if (listResult.IsFailure)
            return Task.FromResult(Result.Failure<IReadOnlyList<DiscoveredVm>>(listResult.Error));

        var names = listResult.Value.StandardOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.Trim())
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        var vms = new List<DiscoveredVm>();
        foreach (var name in names)
        {
            var infoResult = processRunner.RunProcess("virsh",
                ["-c", uri, "dominfo", name], ct);

            var cpuCores = 0;
            var memoryMb = 0;
            if (infoResult.IsSuccess)
            {
                cpuCores = ParseNodeInfoValue(infoResult.Value.StandardOutput, "CPU(s)");
                var memKb = ParseNodeInfoLongValue(infoResult.Value.StandardOutput, "Max memory");
                memoryMb = (int)(memKb / 1024);
            }

            var stateResult = processRunner.RunProcess("virsh",
                ["-c", uri, "domstate", name], ct);
            var powerState = VmPowerState.Unknown;
            if (stateResult.IsSuccess)
            {
                powerState = stateResult.Value.StandardOutput.Trim().ToLower() switch
                {
                    "running" => VmPowerState.Running,
                    "shut off" => VmPowerState.Stopped,
                    "paused" => VmPowerState.Paused,
                    _ => VmPowerState.Unknown
                };
            }

            var macResult = processRunner.RunProcess("virsh",
                ["-c", uri, "domiflist", name, "--all"], ct);
            string? macAddress = null;
            if (macResult.IsSuccess)
            {
                var macMatch = Regex.Match(macResult.Value.StandardOutput,
                    @"([0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2})");
                if (macMatch.Success)
                    macAddress = macMatch.Value;
            }

            vms.Add(new DiscoveredVm(name, name, cpuCores, memoryMb, macAddress, powerState));
        }

        return Task.FromResult(Result.Success<IReadOnlyList<DiscoveredVm>>(vms));
    }

    private static string BuildConnectionUri(VmHostCredential credential)
    {
        var extraConfig = ParseExtraConfig(credential.ExtraConfig);
        if (extraConfig.TryGetValue("uri", out var customUri))
            return customUri;

        var user = credential.Username ?? "root";
        var host = credential.Host;
        var port = credential.Port.HasValue ? $":{ credential.Port.Value}" : "";

        return $"qemu+ssh://{user}@{host}{port}/system";
    }

    private static Dictionary<string, string> ParseExtraConfig(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                   ?? new Dictionary<string, string>();
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }

    private static int ParseNodeInfoValue(string output, string key)
    {
        var match = Regex.Match(output, $@"{Regex.Escape(key)}\s*:\s*(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    private static long ParseNodeInfoLongValue(string output, string key)
    {
        var match = Regex.Match(output, $@"{Regex.Escape(key)}\s*:\s*(\d+)");
        return match.Success ? long.Parse(match.Groups[1].Value) : 0;
    }

    private static double ParsePoolSizeGb(string output, string key)
    {
        var match = Regex.Match(output, $@"{Regex.Escape(key)}\s*:\s*([\d.]+)\s*(\w+)");
        if (!match.Success) return 0;

        var value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
        var unit = match.Groups[2].Value.ToUpper();

        return unit switch
        {
            "GIB" or "GB" => value,
            "TIB" or "TB" => value * 1024,
            "MIB" or "MB" => value / 1024,
            _ => value
        };
    }
}

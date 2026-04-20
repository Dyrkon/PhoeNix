using System.ComponentModel;
using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using PhoeNix.Application.Machines.Commands;
using PhoeNix.Application.Machines.Queries;
using PhoeNix.Contracts.Machines;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;

namespace PhoeNix.McpServer.Tools;

[McpServerToolType]
public static class MachineTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [McpServerTool]
    [Description(
        "List all registered NixOS machines. Returns id, title, MAC address, architecture, machine state, and installed configuration for each.")]
    public static async Task<string> ListMachines(
        ISender sender,
        [Description("Page number (1-based)")] int page = 1,
        [Description("Number of items per page")]
        int pageSize = 15,
        [Description("Optional search term to filter by title or MAC")]
        string? search = null,
        [Description("Filter by enabled status (true/false), omit for all")]
        bool? enabled = null,
        [Description("Filter by architecture: X86_64, Aarch64, Armv7, RiscV64 (optional)")]
        string? architecture = null,
        [Description("Filter by state: Unknown, Registered, Probed, Installed, OutOfDate, UpToDate (optional)")]
        string? machineState = null,
        CancellationToken cancellationToken = default)
    {
        Architecture? arch = null;
        if (!string.IsNullOrEmpty(architecture))
        {
            if (!Enum.TryParse<Architecture>(architecture, true, out var parsed))
                throw new InvalidOperationException($"Unknown architecture '{architecture}'.");
            arch = parsed;
        }

        MachineState? state = null;
        if (!string.IsNullOrEmpty(machineState))
        {
            if (!Enum.TryParse<MachineState>(machineState, true, out var parsed))
                throw new InvalidOperationException($"Unknown machine state '{machineState}'.");
            state = parsed;
        }

        var request = new ListMachinesRequest(
            Page: page,
            PageSize: pageSize,
            Search: search,
            Enabled: enabled,
            Architecture: arch,
            MachineState: state);

        var result = await sender.Send(new ListMachinesQuery(request), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Get detailed information about a machine including hardware inventory (CPU, RAM, GPUs, disks, peripherals), deployment snapshot, and current provisioning status.")]
    public static async Task<string> GetMachine(
        ISender sender,
        [Description("Machine ID (GUID)")] Guid machineId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetMachineQuery(new MachineId(machineId)), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description(
        "Register a new NixOS machine. Returns the new machine ID. The machine must be reachable via PXE boot on the specified MAC address for provisioning.")]
    public static async Task<string> CreateMachine(
        ISender sender,
        [Description("Display name for the machine")]
        string title,
        [Description("MAC address (e.g. 'aa:bb:cc:dd:ee:ff')")]
        string macAddress,
        [Description("Target architecture: X86_64, Aarch64, Armv7, RiscV64")]
        string architecture,
        [Description("Whether this machine is enabled for provisioning")]
        bool enabled = true,
        [Description("Disk selection strategy: Largest, Smallest, First (default: Largest)")]
        string installDiskSelectionPreference = "Largest",
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<Architecture>(architecture, true, out var arch))
            throw new InvalidOperationException($"Unknown architecture '{architecture}'.");

        if (!Enum.TryParse<InstallDiskSelectionPreference>(installDiskSelectionPreference, true, out var diskPref))
            throw new InvalidOperationException(
                $"Unknown disk selection preference '{installDiskSelectionPreference}'. Valid values: {string.Join(", ", Enum.GetNames<InstallDiskSelectionPreference>())}");

        var result = await sender.Send(
            new CreateMachineCommand(title, enabled, macAddress, arch, diskPref),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(new { machineId = result.Value }, JsonOptions);
    }

    [McpServerTool]
    [Description("""
                 Get live Prometheus metrics for a machine. Returns CPU, RAM, disk usage, and network I/O time series.
                 Range options: 1h, 6h, 24h (default), 7d, 30d.
                 Returns isUp (bool) and uptime string, plus metric series with timestamps and values.
                 """)]
    public static async Task<string> GetMachineMetrics(
        ISender sender,
        [Description("Machine ID (GUID)")] Guid machineId,
        [Description("Time range: 1h, 6h, 24h, 7d, 30d (default: 24h)")]
        string range = "24h",
        CancellationToken cancellationToken = default)
    {
        var timeRange = range switch
        {
            "1h" => MetricsTimeRange.OneHour,
            "6h" => MetricsTimeRange.SixHours,
            "7d" => MetricsTimeRange.SevenDays,
            "30d" => MetricsTimeRange.ThirtyDays,
            _ => MetricsTimeRange.TwentyFourHours
        };

        var result = await sender.Send(
            new GetMachineMetricsQuery(new MachineId(machineId), timeRange),
            cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }
}
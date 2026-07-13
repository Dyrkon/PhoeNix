using System.ComponentModel;
using System.Text.Json;
using MediatR;
using ModelContextProtocol.Server;
using PhoeNix.Application.VmHosts.Commands;
using PhoeNix.Application.VmHosts.Queries;
using PhoeNix.Domain.Enums;

namespace PhoeNix.McpServer.Tools;

[McpServerToolType]
public static class VmHostTools
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    [McpServerTool]
    [Description("List all registered VM hosts (hypervisors) with their provider type, resource usage, and linked machine count.")]
    public static async Task<string> ListVmHosts(
        ISender sender,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new ListVmHostsQuery(), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("Get detailed information about a VM host including credentials, resources, and linked machine count.")]
    public static async Task<string> GetVmHost(
        ISender sender,
        [Description("VM host ID (GUID)")] Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetVmHostQuery(vmHostId), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }

    [McpServerTool]
    [Description("Register a new VM host (hypervisor). Tests connection before registering. Provider: 'Libvirt' or 'Proxmox'. For Libvirt: host is SSH hostname, username is SSH user. For Proxmox: host is API URL (https://...:8006), username is API token user (user@realm!tokenid), secret is API token secret. ExtraConfig is optional JSON with provider-specific settings like {\"storagePool\":\"default\",\"network\":\"br0\"} for Libvirt or {\"node\":\"pve\",\"storage\":\"local-lvm\",\"bridge\":\"vmbr0\"} for Proxmox.")]
    public static async Task<string> RegisterVmHost(
        ISender sender,
        [Description("Display name for the VM host")] string name,
        [Description("Provider type: Libvirt or Proxmox")] string provider,
        [Description("Host address (SSH host for Libvirt, API URL for Proxmox)")] string host,
        [Description("Port number (optional)")] int? port = null,
        [Description("Username (SSH user for Libvirt, API token user for Proxmox)")] string? username = null,
        [Description("Secret (SSH key path for Libvirt, API token secret for Proxmox)")] string? secret = null,
        [Description("Extra config as JSON string (optional)")] string? extraConfig = null,
        CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<VmHostProvider>(provider, true, out var providerType))
            throw new InvalidOperationException($"Unknown provider '{provider}'. Use 'Libvirt' or 'Proxmox'.");

        var command = new RegisterVmHostCommand(name, providerType, host, port, username, secret, extraConfig);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return $"VM host registered with ID: {result.Value}";
    }

    [McpServerTool]
    [Description("Remove a registered VM host. Fails if machines still reference it.")]
    public static async Task<string> RemoveVmHost(
        ISender sender,
        [Description("VM host ID (GUID)")] Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new RemoveVmHostCommand(vmHostId), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return "VM host removed successfully.";
    }

    [McpServerTool]
    [Description("Test connectivity to a registered VM host.")]
    public static async Task<string> TestVmHostConnection(
        ISender sender,
        [Description("VM host ID (GUID)")] Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new TestVmHostConnectionCommand(vmHostId), cancellationToken);

        return result.IsFailure
            ? $"Connection failed: {result.Error.Description ?? result.Error.Code}"
            : "Connection successful.";
    }

    [McpServerTool]
    [Description("Refresh resource data (CPU, RAM, storage) from a VM host.")]
    public static async Task<string> SyncVmHostResources(
        ISender sender,
        [Description("VM host ID (GUID)")] Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new SyncVmHostResourcesCommand(vmHostId), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return "Resources synced successfully.";
    }

    [McpServerTool]
    [Description("Discover VMs on a VM host and show which are linked to PhoeNix machines and which are not.")]
    public static async Task<string> DiscoverVms(
        ISender sender,
        [Description("VM host ID (GUID)")] Guid vmHostId,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new DiscoverVmsQuery(vmHostId), cancellationToken);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error.Description ?? result.Error.Code);

        return JsonSerializer.Serialize(result.Value, JsonOptions);
    }
}

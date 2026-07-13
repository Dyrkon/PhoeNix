using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Nodes;
using PhoeNix.Application.Abstractions.Virtualization;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.Virtualization;

public sealed class ProxmoxProvider(IHttpClientFactory httpClientFactory) : IVirtualizationProvider
{
    public VmHostProvider ProviderType => VmHostProvider.Proxmox;

    public async Task<Result> TestConnectionAsync(VmHostCredential credential, CancellationToken ct)
    {
        try
        {
            var client = CreateClient(credential);
            var response = await client.GetAsync("/api2/json/version", ct);
            response.EnsureSuccessStatusCode();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Proxmox.ConnectionFailed",
                $"Failed to connect to Proxmox: {ex.Message}"));
        }
    }

    public async Task<Result<VmHostResources>> GetResourcesAsync(VmHostCredential credential, CancellationToken ct)
    {
        try
        {
            var client = CreateClient(credential);
            var node = GetNodeName(credential);

            var response = await client.GetAsync($"/api2/json/nodes/{node}/status", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var doc = JsonNode.Parse(json);
            var data = doc?["data"];

            var cpuCores = data?["cpuinfo"]?["cpus"]?.GetValue<int>() ?? 0;
            var totalMemory = data?["memory"]?["total"]?.GetValue<long>() ?? 0;
            var usedMemory = data?["memory"]?["used"]?.GetValue<long>() ?? 0;

            long totalStorageGb = 0;
            long usedStorageGb = 0;

            var storageResponse = await client.GetAsync($"/api2/json/nodes/{node}/storage", ct);
            if (storageResponse.IsSuccessStatusCode)
            {
                var storageJson = await storageResponse.Content.ReadAsStringAsync(ct);
                var storageDoc = JsonNode.Parse(storageJson);
                var storages = storageDoc?["data"]?.AsArray() ?? [];

                foreach (var storage in storages)
                {
                    totalStorageGb += (storage?["total"]?.GetValue<long>() ?? 0) / (1024L * 1024 * 1024);
                    usedStorageGb += (storage?["used"]?.GetValue<long>() ?? 0) / (1024L * 1024 * 1024);
                }
            }

            var resources = VmHostResources.Create(
                cpuCores, 0,
                totalMemory / (1024 * 1024), usedMemory / (1024 * 1024),
                totalStorageGb, usedStorageGb);

            return Result.Success(resources);
        }
        catch (Exception ex)
        {
            return Result.Failure<VmHostResources>(new Error("Proxmox.GetResourcesFailed",
                $"Failed to get resources: {ex.Message}"));
        }
    }

    public async Task<Result<CreatedVmInfo>> CreateVmAsync(
        VmHostCredential credential, VmDefinition definition, CancellationToken ct)
    {
        try
        {
            var client = CreateClient(credential);
            var node = GetNodeName(credential);

            var nextIdResponse = await client.GetAsync("/api2/json/cluster/nextid", ct);
            nextIdResponse.EnsureSuccessStatusCode();
            var nextIdJson = await nextIdResponse.Content.ReadAsStringAsync(ct);
            var vmId = JsonNode.Parse(nextIdJson)?["data"]?.GetValue<string>() ?? "100";

            var extraConfig = ParseExtraConfig(credential.ExtraConfig);
            var storageName = extraConfig.GetValueOrDefault("storage", "local-lvm");
            var networkBridge = definition.NetworkBridge ?? extraConfig.GetValueOrDefault("bridge", "vmbr0");

            var createParams = new Dictionary<string, string>
            {
                ["vmid"] = vmId,
                ["name"] = definition.Name,
                ["cores"] = definition.CpuCores.ToString(),
                ["memory"] = definition.MemoryMb.ToString(),
                ["scsi0"] = $"{storageName}:{definition.DiskSizeGb}",
                ["net0"] = $"e1000e,bridge={networkBridge}",
                ["scsihw"] = "virtio-scsi-single",
                ["bios"] = "ovmf",
                ["efidisk0"] = $"{storageName}:1",
                ["boot"] = "order=scsi0;net0",
                ["ostype"] = "l26",
                ["cpu"] = "host"
            };

            var content = new FormUrlEncodedContent(createParams);
            var response = await client.PostAsync($"/api2/json/nodes/{node}/qemu", content, ct);
            response.EnsureSuccessStatusCode();

            var configResponse = await client.GetAsync($"/api2/json/nodes/{node}/qemu/{vmId}/config", ct);
            var macAddress = "00:00:00:00:00:00";
            if (configResponse.IsSuccessStatusCode)
            {
                var configJson = await configResponse.Content.ReadAsStringAsync(ct);
                var configDoc = JsonNode.Parse(configJson);
                var net0 = configDoc?["data"]?["net0"]?.GetValue<string>() ?? "";
                var macMatch = System.Text.RegularExpressions.Regex.Match(net0,
                    @"([0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2})");
                if (macMatch.Success)
                    macAddress = macMatch.Value;
            }

            return Result.Success(new CreatedVmInfo(vmId, definition.Name, macAddress));
        }
        catch (Exception ex)
        {
            return Result.Failure<CreatedVmInfo>(new Error("Proxmox.CreateVmFailed",
                $"Failed to create VM: {ex.Message}"));
        }
    }

    public async Task<Result> DeleteVmAsync(VmHostCredential credential, string externalId, CancellationToken ct)
    {
        try
        {
            var client = CreateClient(credential);
            var node = GetNodeName(credential);

            await client.PostAsync($"/api2/json/nodes/{node}/qemu/{externalId}/status/stop", null, ct);

            await Task.Delay(2000, ct);

            var response = await client.DeleteAsync($"/api2/json/nodes/{node}/qemu/{externalId}", ct);
            response.EnsureSuccessStatusCode();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Proxmox.DeleteVmFailed",
                $"Failed to delete VM: {ex.Message}"));
        }
    }

    public async Task<Result> PowerActionAsync(
        VmHostCredential credential, string externalId, PowerAction action, CancellationToken ct)
    {
        try
        {
            var client = CreateClient(credential);
            var node = GetNodeName(credential);
            var endpoint = action switch
            {
                PowerAction.Start => "start",
                PowerAction.Stop => "shutdown",
                PowerAction.ForceStop => "stop",
                PowerAction.Reboot => "reboot",
                PowerAction.ForceReboot => "reset",
                _ => throw new ArgumentOutOfRangeException(nameof(action))
            };

            var response = await client.PostAsync(
                $"/api2/json/nodes/{node}/qemu/{externalId}/status/{endpoint}", null, ct);
            response.EnsureSuccessStatusCode();

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Proxmox.PowerActionFailed",
                $"Power action '{action}' failed: {ex.Message}"));
        }
    }

    public async Task<Result<VmPowerState>> GetPowerStateAsync(
        VmHostCredential credential, string externalId, CancellationToken ct)
    {
        try
        {
            var client = CreateClient(credential);
            var node = GetNodeName(credential);

            var response = await client.GetAsync(
                $"/api2/json/nodes/{node}/qemu/{externalId}/status/current", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var status = JsonNode.Parse(json)?["data"]?["status"]?.GetValue<string>() ?? "";

            var state = status.ToLower() switch
            {
                "running" => VmPowerState.Running,
                "stopped" => VmPowerState.Stopped,
                "paused" => VmPowerState.Paused,
                _ => VmPowerState.Unknown
            };

            return Result.Success(state);
        }
        catch
        {
            return Result.Success(VmPowerState.Unknown);
        }
    }

    public async Task<Result<IReadOnlyList<DiscoveredVm>>> ListVmsAsync(
        VmHostCredential credential, CancellationToken ct)
    {
        try
        {
            var client = CreateClient(credential);
            var node = GetNodeName(credential);

            var response = await client.GetAsync($"/api2/json/nodes/{node}/qemu", ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var data = JsonNode.Parse(json)?["data"]?.AsArray() ?? [];

            var vms = new List<DiscoveredVm>();
            foreach (var vm in data)
            {
                var vmId = vm?["vmid"]?.GetValue<int>().ToString() ?? "";
                var name = vm?["name"]?.GetValue<string>() ?? vmId;
                var cpus = vm?["cpus"]?.GetValue<int>() ?? 0;
                var maxMem = vm?["maxmem"]?.GetValue<long>() ?? 0;
                var status = vm?["status"]?.GetValue<string>() ?? "";

                var powerState = status.ToLower() switch
                {
                    "running" => VmPowerState.Running,
                    "stopped" => VmPowerState.Stopped,
                    "paused" => VmPowerState.Paused,
                    _ => VmPowerState.Unknown
                };

                string? macAddress = null;
                try
                {
                    var configResponse = await client.GetAsync(
                        $"/api2/json/nodes/{node}/qemu/{vmId}/config", ct);
                    if (configResponse.IsSuccessStatusCode)
                    {
                        var configJson = await configResponse.Content.ReadAsStringAsync(ct);
                        var configDoc = JsonNode.Parse(configJson);
                        var net0 = configDoc?["data"]?["net0"]?.GetValue<string>() ?? "";
                        var macMatch = System.Text.RegularExpressions.Regex.Match(net0,
                            @"([0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2}:[0-9a-fA-F]{2})");
                        if (macMatch.Success)
                            macAddress = macMatch.Value;
                    }
                }
                catch
                {
                    // MAC address lookup is best-effort
                }

                vms.Add(new DiscoveredVm(vmId, name, cpus, (int)(maxMem / (1024 * 1024)),
                    macAddress, powerState));
            }

            return Result.Success<IReadOnlyList<DiscoveredVm>>(vms);
        }
        catch (Exception ex)
        {
            return Result.Failure<IReadOnlyList<DiscoveredVm>>(new Error("Proxmox.ListVmsFailed",
                $"Failed to list VMs: {ex.Message}"));
        }
    }

    private HttpClient CreateClient(VmHostCredential credential)
    {
        var client = httpClientFactory.CreateClient("Proxmox");
        client.BaseAddress = new Uri(credential.Host.TrimEnd('/'));

        if (!string.IsNullOrEmpty(credential.Username) && !string.IsNullOrEmpty(credential.Secret))
        {
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("PVEAPIToken",
                    $"{credential.Username}={credential.Secret}");
        }

        return client;
    }

    private static string GetNodeName(VmHostCredential credential)
    {
        var extraConfig = ParseExtraConfig(credential.ExtraConfig);
        return extraConfig.GetValueOrDefault("node", "pve");
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
}

using System.Text.Json;
using PhoeNix.Application.Abstractions.HardwareProbing;
using PhoeNix.Application.Models.HardwareProbing;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services.HardwareManagement;

public sealed class HardwareInventoryProjector : IHardwareInventoryProjector
{
    public Result<HardwareProfile> Project(HardwareProbeResult probeResult)
    {
        if (string.IsNullOrWhiteSpace(probeResult.RawReport))
            return Result.Failure<HardwareProfile>(new Error(
                "HardwareInventoryProjectionEmptyReport",
                "The hardware report is empty."));

        try
        {
            using var document = JsonDocument.Parse(probeResult.RawReport);
            var root = document.RootElement;

            if (!root.TryGetProperty("hardware", out var hardware))
                return Result.Failure<HardwareProfile>(new Error(
                    "HardwareInventoryProjectionMissingHardwareSection",
                    "The hardware report does not contain the 'hardware' section."));

            var smbios = root.TryGetProperty("smbios", out var smbiosElement)
                ? smbiosElement
                : default;

            var cpu = CreateCpuProfile(hardware);
            var motherboard = CreateMotherboardProfile(smbios);
            var memory = CreateMemoryProfile(hardware, smbios);
            var gpus = CreateGpuProfiles(hardware);
            var disks = CreateDiskProfiles(hardware);
            var peripherals = CreatePeripheralProfiles(hardware);

            var profile = HardwareProfile.Create(
                probeResult.ObservedAtUtc,
                cpu,
                motherboard,
                memory,
                gpus,
                disks,
                peripherals);

            return Result.Success(profile);
        }
        catch (JsonException e)
        {
            return Result.Failure<HardwareProfile>(new Error(
                "HardwareInventoryProjectionInvalidJson",
                $"The hardware report is not valid JSON. {e.Message}"));
        }
        catch (Exception e)
        {
            return Result.Failure<HardwareProfile>(new Error(
                "HardwareInventoryProjectionFailed",
                $"Unable to project the hardware report. {e.Message}"));
        }
    }

    private static CpuProfile? CreateCpuProfile(JsonElement hardware)
    {
        var cpuArray = GetArrayProperty(hardware, "cpu");
        if (cpuArray.ValueKind != JsonValueKind.Array || cpuArray.GetArrayLength() == 0)
            return null;

        var firstCpu = cpuArray[0];

        var vendor = GetString(firstCpu, "vendor_name");
        var model = GetString(firstCpu, "model_name");
        var coreCount = GetInt32(firstCpu, "cores");
        var threadCount = GetInt32(firstCpu, "units");

        return CpuProfile.Create(vendor, model, coreCount, threadCount);
    }

    private static MotherboardProfile? CreateMotherboardProfile(JsonElement smbios)
    {
        if (smbios.ValueKind != JsonValueKind.Object)
            return null;

        if (!smbios.TryGetProperty("board", out var board))
            return null;

        var vendor = GetString(board, "manufacturer");
        var model = GetString(board, "product");

        if (vendor is null && model is null)
            return null;

        return MotherboardProfile.Create(vendor, model);
    }

    private static MemoryProfile? CreateMemoryProfile(JsonElement hardware, JsonElement smbios)
    {
        long? totalBytes = null;
        int? slotCount = null;
        int? occupiedSlotCount = null;
        var modules = new List<MemoryModuleProfile>();

        var memoryArray = GetArrayProperty(hardware, "memory");
        if (memoryArray.ValueKind == JsonValueKind.Array && memoryArray.GetArrayLength() > 0)
        {
            var memory = memoryArray[0];
            var resources = GetArrayProperty(memory, "resources");

            foreach (var resource in resources.EnumerateArray())
            {
                var type = GetString(resource, "type");
                if (!string.Equals(type, "phys_mem", StringComparison.OrdinalIgnoreCase))
                    continue;

                totalBytes = GetInt64(resource, "range");
                break;
            }
        }

        if (smbios.ValueKind == JsonValueKind.Object)
        {
            var memoryArrays = GetArrayProperty(smbios, "memory_array");
            if (memoryArrays.ValueKind == JsonValueKind.Array && memoryArrays.GetArrayLength() > 0)
                slotCount = GetInt32(memoryArrays[0], "slots");

            var memoryDevices = GetArrayProperty(smbios, "memory_device");
            if (memoryDevices.ValueKind == JsonValueKind.Array)
                foreach (var device in memoryDevices.EnumerateArray())
                {
                    var sizeMiB = GetInt64(device, "size");
                    var location = GetString(device, "location");

                    if (sizeMiB is > 0)
                    {
                        occupiedSlotCount ??= 0;
                        occupiedSlotCount++;

                        modules.Add(MemoryModuleProfile.Create(
                            location,
                            sizeMiB.Value * 1024L * 1024L));
                    }
                }
        }

        if (totalBytes is null && slotCount is null && occupiedSlotCount is null && modules.Count == 0)
            return null;

        return MemoryProfile.Create(totalBytes, slotCount, occupiedSlotCount, modules);
    }

    private static IReadOnlyCollection<GpuProfile> CreateGpuProfiles(JsonElement hardware)
    {
        var result = new List<GpuProfile>();
        var gpuArray = GetArrayProperty(hardware, "graphics_card");

        if (gpuArray.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var gpu in gpuArray.EnumerateArray())
        {
            var vendor = GetNestedString(gpu, "vendor", "name");
            var model = FirstNonEmpty(
                GetString(gpu, "model"),
                GetNestedString(gpu, "device", "name"));

            var vramBytes = TryGetGpuMemoryBytes(gpu);

            result.Add(GpuProfile.Create(vendor, model, vramBytes));
        }

        return result;
    }

    private static IReadOnlyCollection<DiskProfile> CreateDiskProfiles(JsonElement hardware)
    {
        var result = new List<DiskProfile>();
        var diskArray = GetArrayProperty(hardware, "disk");

        if (diskArray.ValueKind != JsonValueKind.Array)
            return result;

        foreach (var disk in diskArray.EnumerateArray())
        {
            var unixNames = GetStringArray(disk, "unix_device_names");

            var stableDevicePath = unixNames
                                       .FirstOrDefault(path =>
                                           path.StartsWith("/dev/disk/by-id/", StringComparison.Ordinal))
                                   ?? unixNames.FirstOrDefault(path =>
                                       path.StartsWith("/dev/disk/by-path/", StringComparison.Ordinal));

            var kernelDevicePath = unixNames
                .FirstOrDefault(path => path.StartsWith("/dev/", StringComparison.Ordinal)
                                        && !path.StartsWith("/dev/disk/", StringComparison.Ordinal));

            var model = FirstNonEmpty(
                GetString(disk, "model"),
                GetNestedString(disk, "device", "name"));

            var vendor = GetNestedString(disk, "vendor", "name");
            var busType = GetNestedString(disk, "bus_type", "name");
            var sizeBytes = TryGetDiskSizeBytes(disk);
            var isRotational = TryInferRotational(disk, busType, model, stableDevicePath, kernelDevicePath);

            result.Add(DiskProfile.Create(
                stableDevicePath,
                kernelDevicePath,
                model,
                vendor,
                busType,
                sizeBytes,
                isRotational));
        }

        return result;
    }

    private static IReadOnlyCollection<PeripheralProfile> CreatePeripheralProfiles(JsonElement hardware)
    {
        var result = new List<PeripheralProfile>();

        AddPeripherals(
            result,
            GetArrayProperty(hardware, "monitor"),
            PeripheralKind.Display);

        AddPeripherals(
            result,
            GetArrayProperty(hardware, "keyboard"),
            PeripheralKind.Keyboard);

        AddPeripherals(
            result,
            GetArrayProperty(hardware, "mouse"),
            PeripheralKind.Mouse);

        AddPeripherals(
            result,
            GetArrayProperty(hardware, "sound"),
            PeripheralKind.AudioDevice);

        AddPeripherals(
            result,
            GetArrayProperty(hardware, "network_interface"),
            PeripheralKind.NetworkAdapter);

        AddPeripherals(
            result,
            GetArrayProperty(hardware, "usb"),
            PeripheralKind.UsbDevice);

        AddPeripherals(
            result,
            GetArrayProperty(hardware, "bluetooth"),
            PeripheralKind.BluetoothAdapter);

        return result;
    }

    private static void AddPeripherals(
        List<PeripheralProfile> target,
        JsonElement array,
        PeripheralKind kind)
    {
        if (array.ValueKind != JsonValueKind.Array)
            return;

        foreach (var item in array.EnumerateArray())
        {
            var name = FirstNonEmpty(
                GetString(item, "model"),
                GetNestedString(item, "device", "name"),
                GetString(item, "compat_device"));

            target.Add(PeripheralProfile.Create(kind, name, true));
        }
    }

    private static long? TryGetGpuMemoryBytes(JsonElement gpu)
    {
        var resources = GetArrayProperty(gpu, "resources");
        if (resources.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var resource in resources.EnumerateArray())
        {
            var type = GetString(resource, "type");
            if (!string.Equals(type, "memory", StringComparison.OrdinalIgnoreCase))
                continue;

            var range = GetInt64(resource, "range");
            if (range is > 0)
                return range;
        }

        return null;
    }

    private static long? TryGetDiskSizeBytes(JsonElement disk)
    {
        var resources = GetArrayProperty(disk, "resources");
        if (resources.ValueKind != JsonValueKind.Array)
            return null;

        foreach (var resource in resources.EnumerateArray())
        {
            var type = GetString(resource, "type");
            if (!string.Equals(type, "size", StringComparison.OrdinalIgnoreCase))
                continue;

            var unit = GetString(resource, "unit");
            var value1 = GetInt64(resource, "value_1");
            var value2 = GetInt64(resource, "value_2");

            if (value1 is null || value2 is null)
                continue;

            if (string.Equals(unit, "sectors", StringComparison.OrdinalIgnoreCase))
                return value1.Value * value2.Value;

            return value1.Value;
        }

        return null;
    }

    private static bool? TryInferRotational(
        JsonElement disk,
        string? busType,
        string? model,
        string? stableDevicePath,
        string? kernelDevicePath)
    {
        var normalizedBus = Normalize(busType);
        var normalizedModel = Normalize(model);
        var normalizedStablePath = Normalize(stableDevicePath);
        var normalizedKernelPath = Normalize(kernelDevicePath);

        if (normalizedBus.Contains("nvme", StringComparison.Ordinal))
            return false;

        if (normalizedModel.Contains("ssd", StringComparison.Ordinal))
            return false;

        if (normalizedStablePath.Contains("/dev/disk/by-id/nvme-", StringComparison.Ordinal))
            return false;

        if (normalizedKernelPath.StartsWith("/dev/nvme", StringComparison.Ordinal))
            return false;

        if (normalizedBus.Contains("usb", StringComparison.Ordinal))
            return null;

        return true;
    }

    private static JsonElement GetArrayProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object &&
            element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.Array)
            return property;

        return default;
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            return null;

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString(),
            JsonValueKind.Number => property.GetRawText(),
            _ => null
        };
    }

    private static string? GetNestedString(JsonElement element, string propertyName, string nestedPropertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            return null;

        return GetString(property, nestedPropertyName);
    }

    private static int? GetInt32(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
            return value;

        return null;
    }

    private static long? GetInt64(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            return null;

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var value))
            return value;

        if (property.ValueKind == JsonValueKind.String &&
            long.TryParse(property.GetString(), out var parsed))
            return parsed;

        return null;
    }

    private static IReadOnlyCollection<string> GetStringArray(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var property))
            return Array.Empty<string>();

        if (property.ValueKind != JsonValueKind.Array)
            return Array.Empty<string>();

        return property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Cast<string>()
            .ToList();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim();
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
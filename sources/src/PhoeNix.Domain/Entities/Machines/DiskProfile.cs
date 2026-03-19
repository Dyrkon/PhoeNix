namespace PhoeNix.Domain.Entities.Machines;

public sealed class DiskProfile : HardwareProfileBase
{
    private DiskProfile()
    {
    }

    public string? StableDevicePath { get; private set; }

    public string? KernelDevicePath { get; private set; }

    public string? Model { get; private set; }

    public string? Vendor { get; private set; }

    public string? BusType { get; private set; }

    public long? SizeBytes { get; private set; }

    public bool? IsRotational { get; private set; }

    public static DiskProfile Create(
        string? stableDevicePath,
        string? kernelDevicePath,
        string? model,
        string? vendor,
        string? busType,
        long? sizeBytes,
        bool? isRotational)
    {
        return new DiskProfile
        {
            StableDevicePath = Normalize(stableDevicePath),
            KernelDevicePath = Normalize(kernelDevicePath),
            Model = Normalize(model),
            Vendor = Normalize(vendor),
            BusType = Normalize(busType),
            SizeBytes = sizeBytes,
            IsRotational = isRotational
        };
    }
}
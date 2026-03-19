namespace PhoeNix.Domain.Entities.Machines;

public sealed class GpuProfile : HardwareProfileBase
{
    private GpuProfile()
    {
    }

    public string? Vendor { get; private set; }

    public string? Model { get; private set; }

    public long? VramBytes { get; private set; }

    public static GpuProfile Create(
        string? vendor,
        string? model,
        long? vramBytes)
    {
        return new GpuProfile
        {
            Vendor = Normalize(vendor),
            Model = Normalize(model),
            VramBytes = vramBytes
        };
    }
}
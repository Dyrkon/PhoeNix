namespace PhoeNix.Domain.Entities.Machines;

public sealed class CpuProfile : HardwareProfileBase
{
    private CpuProfile()
    {
    }

    public string? Vendor { get; private set; }

    public string? Model { get; private set; }

    public int? CoreCount { get; private set; }

    public int? ThreadCount { get; private set; }

    public static CpuProfile Create(
        string? vendor,
        string? model,
        int? coreCount,
        int? threadCount)
    {
        return new CpuProfile
        {
            Vendor = Normalize(vendor),
            Model = Normalize(model),
            CoreCount = coreCount,
            ThreadCount = threadCount
        };
    }
}
namespace PhoeNix.Domain.Entities.Machines;

public sealed class MemoryModuleProfile : HardwareProfileBase
{
    private MemoryModuleProfile()
    {
    }

    public string? Slot { get; private set; }

    public long? SizeBytes { get; private set; }

    public static MemoryModuleProfile Create(string? slot, long? sizeBytes)
    {
        return new MemoryModuleProfile
        {
            Slot = Normalize(slot),
            SizeBytes = sizeBytes
        };
    }
}
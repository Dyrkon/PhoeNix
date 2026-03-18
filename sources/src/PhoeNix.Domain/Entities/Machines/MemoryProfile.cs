namespace PhoeNix.Domain.Entities.Machines;

public sealed class MemoryProfile
{
    private readonly List<MemoryModuleProfile> _modules = [];

    private MemoryProfile()
    {
    }

    public long? TotalBytes { get; private set; }

    public int? SlotCount { get; private set; }

    public int? OccupiedSlotCount { get; private set; }

    public IReadOnlyCollection<MemoryModuleProfile> Modules => _modules;

    public static MemoryProfile Create(
        long? totalBytes,
        int? slotCount,
        int? occupiedSlotCount,
        IEnumerable<MemoryModuleProfile>? modules)
    {
        var profile = new MemoryProfile
        {
            TotalBytes = totalBytes,
            SlotCount = slotCount,
            OccupiedSlotCount = occupiedSlotCount
        };

        if (modules is not null)
            profile._modules.AddRange(modules);

        return profile;
    }
}
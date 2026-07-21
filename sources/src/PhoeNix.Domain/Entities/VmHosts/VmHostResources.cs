namespace PhoeNix.Domain.Entities.VmHosts;

public sealed record VmHostResources
{
    private VmHostResources()
    {
    }

    public int TotalCpuCores { get; private set; }

    public int UsedCpuCores { get; private set; }

    public long TotalMemoryMb { get; private set; }

    public long UsedMemoryMb { get; private set; }

    public long TotalStorageGb { get; private set; }

    public long UsedStorageGb { get; private set; }

    public static VmHostResources Create(
        int totalCpuCores,
        int usedCpuCores,
        long totalMemoryMb,
        long usedMemoryMb,
        long totalStorageGb,
        long usedStorageGb)
    {
        return new VmHostResources
        {
            TotalCpuCores = totalCpuCores,
            UsedCpuCores = usedCpuCores,
            TotalMemoryMb = totalMemoryMb,
            UsedMemoryMb = usedMemoryMb,
            TotalStorageGb = totalStorageGb,
            UsedStorageGb = usedStorageGb
        };
    }
}

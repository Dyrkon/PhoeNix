using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Machines;

public class HardwareProfile
{
    private readonly List<GpuProfile> _gpus = [];
    private readonly List<DiskProfile> _disks = [];
    private readonly List<PeripheralProfile> _peripherals = [];

    private HardwareProfile()
    {
    }

    public DateTime ObservedAtUtc { get; private set; }

    public CpuProfile? Cpu { get; private set; }

    public MotherboardProfile? Motherboard { get; private set; }

    public MemoryProfile? Memory { get; private set; }

    public IReadOnlyCollection<GpuProfile> Gpus => _gpus;

    public IReadOnlyCollection<DiskProfile> Disks => _disks;

    public IReadOnlyCollection<PeripheralProfile> Peripherals => _peripherals;

    public bool HasConnectedDisplay => _peripherals.Any(p => p is { Kind: PeripheralKind.Display, IsConnected: true });

    public static HardwareProfile Create(
        DateTime observedAtUtc,
        CpuProfile? cpu,
        MotherboardProfile? motherboard,
        MemoryProfile? memory,
        IEnumerable<GpuProfile>? gpus,
        IEnumerable<DiskProfile>? disks,
        IEnumerable<PeripheralProfile>? peripherals)
    {
        var profile = new HardwareProfile
        {
            ObservedAtUtc = observedAtUtc,
            Cpu = cpu,
            Motherboard = motherboard,
            Memory = memory
        };

        if (gpus is not null)
            profile._gpus.AddRange(gpus);

        if (disks is not null)
            profile._disks.AddRange(disks);

        if (peripherals is not null)
            profile._peripherals.AddRange(peripherals);

        return profile;
    }
}
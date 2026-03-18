using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Entities.Machines;

public sealed class PeripheralProfile : HardwareProfileBase
{
    private PeripheralProfile()
    {
    }

    public PeripheralKind Kind { get; private set; }

    public string? Name { get; private set; }

    public bool IsConnected { get; private set; }

    public static PeripheralProfile Create(
        PeripheralKind kind,
        string? name,
        bool isConnected)
    {
        return new PeripheralProfile
        {
            Kind = kind,
            Name = Normalize(name),
            IsConnected = isConnected
        };
    }
}
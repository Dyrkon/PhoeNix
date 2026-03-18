namespace PhoeNix.Domain.Entities.Machines;

public sealed class MotherboardProfile : HardwareProfileBase
{
    private MotherboardProfile()
    {
    }

    public string? Vendor { get; private set; }

    public string? Model { get; private set; }

    public static MotherboardProfile Create(string? vendor, string? model)
    {
        return new MotherboardProfile
        {
            Vendor = Normalize(vendor),
            Model = Normalize(model)
        };
    }
}
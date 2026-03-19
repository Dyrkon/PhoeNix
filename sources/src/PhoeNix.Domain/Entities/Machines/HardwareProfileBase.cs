namespace PhoeNix.Domain.Entities.Machines;

public abstract class HardwareProfileBase
{
    protected static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
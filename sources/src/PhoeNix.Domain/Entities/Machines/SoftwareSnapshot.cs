using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.Entities.Machines;

public class SoftwareSnapshot
{
    public int? SchemaVersion { get; private set; } = 1;
}
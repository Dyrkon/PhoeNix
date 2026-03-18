using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Application.Models.HardwareProbing;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.HardwareProbing;

public interface IHardwareInventoryProjector
{
    Result<HardwareProfile> Project(HardwareProbeResult probeResult);
}
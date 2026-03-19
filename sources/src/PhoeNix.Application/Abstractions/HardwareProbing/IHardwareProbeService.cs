using PhoeNix.Application.Models.HardwareProbing;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.HardwareProbing;

public interface IHardwareProbeService
{
    Task<Result<HardwareProbeResult>> ProbeAsync(
        SetupSession session,
        MachineId machineId,
        CancellationToken cancellationToken);
}
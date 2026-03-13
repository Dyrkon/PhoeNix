using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;

namespace PhoeNix.Domain.Repositories;

public interface IProvisioningSessionRepository : IRepository<ProvisioningSession, ProvisioningSessionId>
{
    Task<ProvisioningSession?> GetWithEnrolledMachineAsync(MachineId machineId, CancellationToken cancellationToken);
}
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;

namespace PhoeNix.Application.Repositories;

public interface ISetupSessionRepository : IRepository<SetupSession, SetupSessionId>
{
    Task<SetupSession?> GetWithEnrolledMachineAsync(MachineId machineId, CancellationToken cancellationToken);
}
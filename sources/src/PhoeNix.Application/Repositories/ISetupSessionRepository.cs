using PhoeNix.Application.Models.Setup;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Application.Repositories;

public interface ISetupSessionRepository : IRepository<SetupSession, SetupSessionId>
{
    Task<SetupSession?> GetWithEnrolledMachineAsync(MachineId machineId, CancellationToken cancellationToken);

    Task<PagedResponse<SetupSession>> GetSetupSessions(
        SetupSessionsRequest sessionsRequest,
        UserId ownerId,
        CancellationToken cancellationToken);
}
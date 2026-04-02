using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;

namespace PhoeNix.Persistence.Repositories;

public class SetupSessionRepository : RepositoryBase<SetupSession, SetupSessionId>,
    ISetupSessionRepository
{
    public SetupSessionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<SetupSession?> GetWithEnrolledMachineAsync(MachineId machineId,
        CancellationToken cancellationToken)
    {
        return DbContext
            .Set<SetupSession>()
            .OrderByDescending(s => s.StartTime)
            .FirstOrDefaultAsync(
                p => p.Targets.Any(t => t.MachineId == machineId),
                cancellationToken);
    }
}
using Microsoft.EntityFrameworkCore;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

public class ProvisioningSessionRepository : RepositoryBase<ProvisioningSession, ProvisioningSessionId>,
    IProvisioningSessionRepository
{
    public ProvisioningSessionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public Task<ProvisioningSession?> GetWithEnrolledMachineAsync(MachineId machineId,
        CancellationToken cancellationToken)
    {
        return DbContext
            .Set<ProvisioningSession>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                p => p.Targets.Any(t => t.MachineId == machineId),
                cancellationToken);
    }
}
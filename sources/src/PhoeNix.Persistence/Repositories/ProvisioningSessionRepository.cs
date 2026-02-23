using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Domain.Repositories;

namespace PhoeNix.Persistence.Repositories;

public class ProvisioningSessionRepository : RepositoryBase<ProvisioningSession, ProvisioningSessionId>,
    IProvisioningSessionRepository
{
    public ProvisioningSessionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
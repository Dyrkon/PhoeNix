using Microsoft.EntityFrameworkCore;
using PhoeNix.Application.Monitoring.GetPrometheusTargets;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Persistence.Repositories;

public sealed class PrometheusTargetsRepository(ApplicationDbContext dbContext) : IPrometheusTargetsRepository
{
    private static readonly MachineState[] ActiveStates =
    [
        MachineState.Orchestrated,
        MachineState.Updated,
        MachineState.OutDated
    ];

    public async Task<IReadOnlyList<PrometheusTarget>> GetTargetsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Machines
            .AsNoTracking()
            .Where(m => ActiveStates.Contains(m.MachineStatus.MachineState) && m.DeploymentSnapshot != null)
            .Select(m => new PrometheusTarget(
                m.Title,
                m.DeploymentSnapshot!.LastKnownIpAddress,
                "9100"))
            .ToListAsync(cancellationToken);
    }
}

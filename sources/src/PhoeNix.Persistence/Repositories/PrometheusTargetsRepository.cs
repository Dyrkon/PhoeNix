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

    public async Task<IReadOnlyList<PrometheusTarget>> GetTargetsAsync(
        MonitoringAddressResolution resolution,
        CancellationToken cancellationToken)
    {
        if (resolution == MonitoringAddressResolution.LastKnownIp)
        {
            var machines = await dbContext.Machines
                .AsNoTracking()
                .Include(m => m.DeploymentSnapshot)
                .Where(m => ActiveStates.Contains(m.MachineStatus.MachineState) && m.DeploymentSnapshot != null)
                .ToListAsync(cancellationToken);

            return machines
                .Select(m => new PrometheusTarget(
                    m.Title,
                    m.DeploymentSnapshot!.LastKnownIpAddress.ToString(),
                    "9100"))
                .ToList();
        }

        return await dbContext.Machines
            .AsNoTracking()
            .Where(m => ActiveStates.Contains(m.MachineStatus.MachineState) && m.DeploymentSnapshot != null)
            .Select(m => new PrometheusTarget(
                m.Title,
                m.Title + ".local",
                "9100"))
            .ToListAsync(cancellationToken);
    }
}

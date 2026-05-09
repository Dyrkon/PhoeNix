using PhoeNix.Application.Monitoring.GetPrometheusTargets;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Repositories;

public interface IPrometheusTargetsRepository
{
    Task<IReadOnlyList<PrometheusTarget>> GetTargetsAsync(MonitoringAddressResolution resolution, CancellationToken cancellationToken);
}

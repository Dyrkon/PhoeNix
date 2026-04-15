using PhoeNix.Application.Monitoring.GetPrometheusTargets;

namespace PhoeNix.Application.Repositories;

public interface IPrometheusTargetsRepository
{
    Task<IReadOnlyList<PrometheusTarget>> GetTargetsAsync(CancellationToken cancellationToken);
}

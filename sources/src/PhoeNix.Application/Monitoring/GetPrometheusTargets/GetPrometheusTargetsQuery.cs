using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Monitoring.GetPrometheusTargets;

public sealed record PrometheusTarget(string Title, string Address, string MetricsPort);

public sealed record GetPrometheusTargetsQuery : IQuery<IReadOnlyList<PrometheusTarget>>;

internal sealed class GetPrometheusTargetsQueryHandler(
    IPrometheusTargetsRepository repository,
    IAppSettingsRepository appSettingsRepository)
    : IQueryHandler<GetPrometheusTargetsQuery, IReadOnlyList<PrometheusTarget>>
{
    public async Task<Result<IReadOnlyList<PrometheusTarget>>> Handle(
        GetPrometheusTargetsQuery request,
        CancellationToken cancellationToken)
    {
        var settings = await appSettingsRepository.GetFirstAsync(cancellationToken);
        var resolution = settings?.MonitoringAddressResolution ?? MonitoringAddressResolution.MdnsHostname;
        var targets = await repository.GetTargetsAsync(resolution, cancellationToken);
        return Result.Success(targets);
    }
}

using System.Net;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Monitoring.GetPrometheusTargets;

public sealed record PrometheusTarget(string Title, IPAddress IpAddress, string MetricsPort);

public sealed record GetPrometheusTargetsQuery : IQuery<IReadOnlyList<PrometheusTarget>>;

internal sealed class GetPrometheusTargetsQueryHandler(IPrometheusTargetsRepository repository)
    : IQueryHandler<GetPrometheusTargetsQuery, IReadOnlyList<PrometheusTarget>>
{
    public async Task<Result<IReadOnlyList<PrometheusTarget>>> Handle(
        GetPrometheusTargetsQuery request,
        CancellationToken cancellationToken)
    {
        var targets = await repository.GetTargetsAsync(cancellationToken);
        return Result.Success(targets);
    }
}

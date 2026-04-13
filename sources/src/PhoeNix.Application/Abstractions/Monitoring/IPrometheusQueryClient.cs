namespace PhoeNix.Application.Abstractions.Monitoring;

public record PrometheusRangeSeries(DateTimeOffset[] Timestamps, double?[] Values);

public interface IPrometheusQueryClient
{
    Task<double?> QueryInstantAsync(string promQl, CancellationToken cancellationToken = default);
    Task<PrometheusRangeSeries> QueryRangeAsync(string promQl, DateTimeOffset start, DateTimeOffset end, TimeSpan step, CancellationToken cancellationToken = default);
}

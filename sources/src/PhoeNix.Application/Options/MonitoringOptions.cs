namespace PhoeNix.Application.Options;

public sealed class MonitoringOptions
{
    public string? StateDir { get; init; }
    public TimeSpan TokenTtl { get; init; } = TimeSpan.FromDays(7);
    public string PrometheusEndpoint { get; init; } = "http://localhost:9090/prometheus";
}
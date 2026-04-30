namespace PhoeNix.Application.Options;

public sealed class NetbootHostOptions
{
    public string ApiBasePublicUrl { get; init; } = "http://YOUR-API-OR-HOSTNAME:8888/api";
    public string ApiBaseUrl { get; init; } = "http://0.0.0.0:5001";
    public string HostExecutablePath { get; init; } = "/run/wrappers/bin/pixiecore";

    public string ListenAddress { get; init; } = "0.0.0.0";

    public int Port { get; init; } = 64172;

    public int StatusPort { get; init; } = 64173;

    public TimeSpan HealthCheckInterval { get; init; } = TimeSpan.FromSeconds(10);
}
namespace PhoeNix.Application.Options;

public sealed class NetbootHostOptions
{
    public string HostExecutablePath { get; init; } = "pixiecore";

    public string ListenAddress { get; init; } = "0.0.0.0";

    public int Port { get; init; } = 64172;

    public int StatusPort { get; init; } = 64173;

    public TimeSpan HealthCheckInterval { get; init; } = TimeSpan.FromSeconds(10);
}

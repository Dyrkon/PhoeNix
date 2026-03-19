namespace PhoeNix.Application.Options;

public sealed class HardwareProbeOptions
{
    public string SshExecutable { get; init; } = "ssh";

    public string BootstrapUser { get; init; } = "root";

    public string ProbeCommand { get; init; } = "nixos-facter";

    public int ConnectTimeoutSeconds { get; init; } = 10;

    public int ProbeTimeoutSeconds { get; init; } = 60;

    public bool DisableHostKeyChecking { get; init; } = true;
}
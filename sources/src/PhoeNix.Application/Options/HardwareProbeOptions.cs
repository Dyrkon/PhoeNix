namespace PhoeNix.Application.Options;

public sealed class HardwareProbeOptions
{
    public string SshExecutable { get; init; } = "ssh";

    public string BootstrapUser { get; init; } = "root";

    public string ProbeCommand { get; init; } = "nixos-facter";

    public int ConnectTimeoutSeconds { get; init; } = 30;

    public int ProbeTimeoutSeconds { get; init; } = 120;

    public bool DisableHostKeyChecking { get; init; } = true;
}
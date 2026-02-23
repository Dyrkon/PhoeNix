using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.Options;

public sealed class BootstrapArtifactsOptions
{
    public List<BootstrapBaseImageOptions> BaseImages { get; init; } = [];

    public string WorkDirectory { get; init; } = "phoenix-bootstrap";

    public string NixStoreExecutable { get; init; } = "nix-store";

    public string CpioExecutable { get; init; } = "cpio";

    public string GzipExecutable { get; init; } = "gzip";
}

public sealed class BootstrapBaseImageOptions
{
    public Architecture Architecture { get; init; }

    public string KernelPath { get; init; } = string.Empty;

    public string InitrdPath { get; init; } = string.Empty;

    public string KernelParams { get; init; } = string.Empty;
}

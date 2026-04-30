namespace PhoeNix.Application.Options;

public sealed class SeedExampleOptions
{
    public string HostName { get; init; } = "phoenix-demo";
    public string StateVersion { get; init; } = "25.11";
    public List<string> RootAuthorizedKeys { get; init; } = [];
    public string PublicBaseUrl { get; init; } = "http://127.0.0.1:5001";
    public int MetricsPort { get; init; } = 9100;
    public bool OpenFirewall { get; init; } = true;
    public string Timezone { get; init; } = "UTC";

    public List<string> NixSubstituters { get; init; } =
        ["\"https://cache.nixos.org/\"", "\"https://nix-community.cachix.org\""];

    public List<string> NixTrustedPublicKeys { get; init; } =
    [
        "\"cache.nixos.org-1:6NCHdD59X431o0gWypbMrAURkbJ16ZPMQFGspcDShjY=\"",
        "\"nix-community.cachix.org-1:mB9FSh9qf2dCimDSUo8Zy7bkq5CX+/rkCWyvRCUSeBw=\""
    ];

    public int NixMaxJobs { get; init; } = 4;
    public int NixCores { get; init; } = 1;
}
namespace PhoeNix.Application.Options;

public sealed class SeedExampleOptions
{
    public string HostName { get; init; } = "phoenix-demo";
    public string StateVersion { get; init; } = "25.11";
    public List<string> RootAuthorizedKeys { get; init; } = [];
    public string PublicBaseUrl { get; init; } = "http://127.0.0.1:5083";
    public int MetricsPort { get; init; } = 9100;
    public bool OpenFirewall { get; init; } = true;
}
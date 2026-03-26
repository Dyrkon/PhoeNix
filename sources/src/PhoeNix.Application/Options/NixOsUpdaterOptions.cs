namespace PhoeNix.Application.Options;

public sealed class NixOsUpdaterOptions
{
    public string BuildHost { get; set; } = "";
    public bool UseRemoteSudo { get; init; } = true;
    public bool Fast { get; init; } = true;
}
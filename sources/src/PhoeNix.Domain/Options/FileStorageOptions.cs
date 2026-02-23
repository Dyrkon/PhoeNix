namespace PhoeNix.Domain.Options;

public class FileStorageOptions
{
    public string RootPath { get; init; } = ".phoenix";

    public string ConfigurationsPath { get; init; } = "configurations";

    public bool UseTemp { get; init; }
}

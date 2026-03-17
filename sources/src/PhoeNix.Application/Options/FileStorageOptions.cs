namespace PhoeNix.Application.Options;

public class FileStorageOptions
{
    public bool UseTemp { get; set; }
    public string RootPath { get; set; } = "/var/lib/phoenix";
    public string ConfigurationsPath { get; set; } = "configurations";
}
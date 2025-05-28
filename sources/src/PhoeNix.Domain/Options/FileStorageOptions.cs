namespace PhoeNix.Domain.Options;

public class FileStorageOptions
{
    public string RootPath { get; init; }

    public string ConfigurationsPath { get; init; }
    
    public string ModulesPath  { get; init; }
}
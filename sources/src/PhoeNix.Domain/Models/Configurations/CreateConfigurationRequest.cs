namespace PhoeNix.Domain.Models.Configurations;

public record CreateConfigurationRequest
{
    public string Name { get; set; }

    public string Description { get; set; }
}
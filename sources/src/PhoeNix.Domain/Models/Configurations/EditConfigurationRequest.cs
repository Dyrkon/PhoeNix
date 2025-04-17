namespace PhoeNix.Domain.Models.Configurations;

public record EditConfigurationRequest
{
    public string Name { get; set; }

    public string Description { get; set; }
}
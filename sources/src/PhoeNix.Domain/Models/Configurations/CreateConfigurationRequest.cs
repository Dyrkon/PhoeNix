namespace PhoeNix.Domain.Models.Configurations;

public record CreateConfigurationRequest(
    string Title,
    string Description
);
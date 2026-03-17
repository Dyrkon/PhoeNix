namespace PhoeNix.Application.Models.Configurations;

public record CreateConfigurationRequest(
    string Title,
    string Description
);
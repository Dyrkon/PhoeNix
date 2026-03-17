namespace PhoeNix.Application.Models.Configurations;

public record EditConfigurationRequest(
    string? Title,
    string? Description
);
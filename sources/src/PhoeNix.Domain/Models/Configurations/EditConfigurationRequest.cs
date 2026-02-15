namespace PhoeNix.Domain.Models.Configurations;

public record EditConfigurationRequest(
    string? Title,
    string? Description
);
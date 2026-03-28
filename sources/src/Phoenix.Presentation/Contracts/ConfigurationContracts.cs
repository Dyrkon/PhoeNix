namespace Phoenix.Presentation.Contracts;

public sealed record CreateConfigurationRequest(
    string Title,
    string Description);

public sealed record UpdateConfigurationRequest(
    string Title,
    string Description);
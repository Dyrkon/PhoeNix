namespace PhoeNix.WebAPP.ApiClient.Contracts;

public sealed record ConfigurationListResponse(
    Guid Id,
    string Title,
    string Description);

public sealed record ConfigurationResponse(
    Guid Id,
    string Title,
    string Description,
    IReadOnlyList<InputResponse> Inputs,
    IReadOnlyList<ModuleValueResponse> Modules,
    IReadOnlyList<SystemListResponse> Systems,
    IReadOnlyList<Architecture> SupportedArchitectures);

public sealed record CreateConfigurationRequest(
    string? Title,
    string? Description);

public sealed record UpdateConfigurationRequest(
    string? Title,
    string? Description);

public sealed record CreateConfigurationInputRequest(
    string? Source,
    string? Name,
    List<InputFollowUpsertModel>? Follows);

public sealed record UpdateConfigurationInputRequest(
    string? Source,
    string? Name,
    List<InputFollowUpsertModel>? Follows);

public sealed record CreateConfigurationModuleRequest(
    Guid ModuleTemplateId,
    bool Enabled);

public sealed record UpdateConfigurationModuleRequest(
    bool Enabled,
    List<ModuleEntryValueUpsertModel>? Entries);

public sealed record CreateConfigurationSystemRequest(
    string? Name,
    Architecture Architecture);

public sealed record UpdateConfigurationSystemRequest(
    string? Name);

public sealed record CreateConfigurationSystemModuleRequest(
    Guid ModuleTemplateId,
    bool Enabled);

public sealed record UpdateConfigurationSystemModuleRequest(
    bool Enabled,
    List<ModuleEntryValueUpsertModel>? Entries);
using PhoeNix.Application.Models.Inputs;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Enums;

namespace Phoenix.Presentation.Contracts;

public sealed record CreateConfigurationInputRequest(
    string Source,
    string Name,
    IReadOnlyList<InputFollowUpsertModel> Follows);

public sealed record UpdateConfigurationInputRequest(
    string Source,
    string Name,
    IReadOnlyList<InputFollowUpsertModel> Follows);

public sealed record CreateConfigurationSystemRequest(
    string Name,
    Architecture Architecture);

public sealed record UpdateConfigurationSystemRequest(
    string Name);

public sealed record CreateConfigurationModuleRequest(
    Guid ModuleTemplateId,
    bool Enabled);

public sealed record UpdateConfigurationModuleRequest(
    bool Enabled,
    IReadOnlyList<ModuleEntryValueUpsertModel> Entries);

public sealed record CreateConfigurationSystemModuleRequest(
    Guid ModuleTemplateId,
    bool Enabled);

public sealed record UpdateConfigurationSystemModuleRequest(
    bool Enabled,
    IReadOnlyList<ModuleEntryValueUpsertModel> Entries);
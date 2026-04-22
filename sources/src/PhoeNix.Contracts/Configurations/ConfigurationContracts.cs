using PhoeNix.Common.Models;
using PhoeNix.Contracts.Inputs;
using PhoeNix.Contracts.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Contracts.Configurations;

public enum ConfigurationSortField
{
    Title = 0,
    Description = 1
}

public sealed record ConfigurationListResponse(Guid Id, string Title, string Description);

public sealed record ListConfigurationsRequest(
    int Page = 1,
    int PageSize = 15,
    string? Search = null,
    ConfigurationSortField SortField = ConfigurationSortField.Title,
    SortDirection SortDirection = SortDirection.Ascending);

public sealed record ConfigurationResponse(
    Guid Id,
    string Title,
    string Description,
    IReadOnlyList<InputResponse> Inputs,
    IReadOnlyList<ConfiguredModuleResponse> Modules,
    IReadOnlyList<ConfiguredSystemResponse> Systems,
    IReadOnlyList<Architecture> SupportedArchitectures);

public sealed record ConfigurationWithRevisionsResponse(
    Guid Id,
    string Title,
    string Description,
    IReadOnlyList<InputResponse> Inputs,
    IReadOnlyList<ConfiguredModuleResponse> Modules,
    IReadOnlyList<ConfiguredSystemResponse> Systems,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyCollection<ConfigurationRevisionResponse> Revisions);

public sealed record ConfigurationRevisionResponse(
    string Title,
    string Description,
    DateTime TimeStamp,
    int RevisionNumber,
    string JsonSnapshot
);

public sealed record ConfiguredModuleResponse(
    Guid ModuleValueId,
    Guid ModuleTemplateId,
    string TemplateName,
    bool TemplateEnabled,
    bool Enabled,
    ModuleType TemplateType,
    bool RequiresSetupBindings,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ConfiguredModuleEntryResponse> Entries);

public sealed record ConfiguredModuleEntryResponse(
    Guid Id,
    string Name,
    string Placeholder,
    EntryValueKind Kind,
    string? Value,
    int? IntegerLowerValue,
    int? IntegerUpperValue,
    decimal? DecimalLowerValue,
    decimal? DecimalUpperValue,
    IReadOnlyList<string>? Options,
    IReadOnlyList<string>? ListItems = null);

public sealed record ConfiguredSystemResponse(
    Guid Id,
    string Name,
    Architecture Architecture,
    IReadOnlyList<ConfiguredModuleResponse> Modules);

public sealed record CreateConfigurationRequest(string? Title, string? Description);

public sealed record UpdateConfigurationRequest(string? Title, string? Description);

public sealed record CreateConfigurationInputRequest(
    string? Source,
    string? Name,
    List<InputFollowUpsertModel>? Follows);

public sealed record UpdateConfigurationInputRequest(
    string? Source,
    string? Name,
    List<InputFollowUpsertModel>? Follows);

public sealed record CreateConfigurationModuleRequest(Guid ModuleTemplateId, bool Enabled);

public sealed record UpdateConfigurationModuleRequest(
    bool Enabled,
    List<ModuleEntryValueUpsertModel>? Entries);

public sealed record CreateConfigurationSystemRequest(string? Name, Architecture Architecture);

public sealed record UpdateConfigurationSystemRequest(string? Name);

public sealed record CreateConfigurationSystemModuleRequest(Guid ModuleTemplateId, bool Enabled);

public sealed record UpdateConfigurationSystemModuleRequest(
    bool Enabled,
    List<ModuleEntryValueUpsertModel>? Entries);
using PhoeNix.Application.Models.Inputs;
using PhoeNix.Application.Models.Systems;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Configurations;

public sealed record ConfigurationListResponse(
    Guid Id,
    string Title,
    string Description);

public enum ConfigurationSortField
{
    Title = 0,
    Description = 1
}

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
using PhoeNix.Common.Models;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Contracts.Modules;

public sealed record ModuleTemplateListResponse(
    Guid Id,
    string Name,
    bool Enabled,
    ModuleType Type,
    IReadOnlyList<Architecture> SupportedArchitectures);

public sealed record EntryValueDefinitionResponse(
    string Name,
    string Placeholder,
    EntryBindingKind BindingKind,
    EntryValueKind ValueKind,
    string? DefaultValue,
    string? DefaultLowerValue,
    int? IntegerMin,
    int? IntegerMax,
    decimal? DecimalMin,
    decimal? DecimalMax,
    bool AllowLowerValue,
    IReadOnlyList<string>? Options,
    int? BindingIndex);

public sealed record ModuleTemplateTestResponse(
    Guid Id,
    string Name,
    string Content,
    IReadOnlyList<string> VariableNames);

public sealed record RequiredInputDefinitionResponse(string Name, string Source);

public sealed record RequiredInputDefinitionModel(string Name, string Source);

public sealed record ModuleTemplateResponse(
    Guid Id,
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    bool RequiresSetupBindings,
    IReadOnlyList<EntryValueDefinitionResponse> EditableValueTypes,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ModuleTemplateTestResponse> Tests,
    IReadOnlyList<RequiredInputDefinitionResponse>? RequiredInputs = null);

public sealed record ModuleTemplateEntryValueDefinitionModel(
    string? Name,
    string? Placeholder,
    EntryBindingKind BindingKind,
    EntryValueKind ValueKind,
    string? DefaultValue,
    string? DefaultLowerValue,
    int? IntegerMin,
    int? IntegerMax,
    decimal? DecimalMin,
    decimal? DecimalMax,
    bool AllowLowerValue,
    IReadOnlyList<string>? Options,
    int? BindingIndex);

public sealed record ModuleTemplateTestUpsertModel(
    Guid? Id,
    string? Name,
    string? Content,
    IReadOnlyList<string>? VariableNames);

public sealed record ModuleEntryValueUpsertModel(
    string? Name,
    string? Placeholder,
    EntryValueKind Kind,
    string? TextValue,
    int? IntegerUpperValue,
    int? IntegerLowerValue,
    decimal? DecimalUpperValue,
    decimal? DecimalLowerValue,
    string? SelectedValue,
    IReadOnlyList<string>? ListItems = null);

public sealed record EntryValueResponse(
    Guid Id,
    string Name,
    string Placeholder,
    EntryValueKind Kind,
    string Value,
    int? IntegerMin,
    int? IntegerMax,
    int? IntegerLowerValue,
    int? IntegerUpperValue,
    decimal? DecimalMin,
    decimal? DecimalMax,
    decimal? DecimalLowerValue,
    decimal? DecimalUpperValue,
    IReadOnlyList<string>? Options,
    IReadOnlyList<string>? ListItems = null);

public sealed record ModuleValueResponse(Guid Id, bool Enabled, List<EntryValueResponse> EditableValues);

public sealed record ModuleValueListResponse(Guid Id, bool Enabled);

public sealed record CreateModuleTemplateRequest(
    string? Name,
    bool Enabled,
    ModuleType Type,
    string? Content,
    List<Architecture>? SupportedArchitectures,
    List<ModuleTemplateEntryValueDefinitionModel>? EditableValueTypes,
    List<ModuleTemplateTestUpsertModel>? Tests,
    List<RequiredInputDefinitionModel>? RequiredInputs = null);

public sealed record UpdateModuleTemplateRequest(
    string? Name,
    bool Enabled,
    ModuleType Type,
    string? Content,
    List<Architecture>? SupportedArchitectures,
    List<ModuleTemplateEntryValueDefinitionModel>? EditableValueTypes,
    List<ModuleTemplateTestUpsertModel>? Tests,
    List<RequiredInputDefinitionModel>? RequiredInputs = null);

public sealed record ListModuleTemplatesRequest(
    ModuleTemplateSortField SortField = ModuleTemplateSortField.Name,
    int Page = 1,
    int PageSize = 15,
    string? Search = null,
    bool? Enabled = null,
    ModuleType? Type = null,
    SortDirection SortDirection = SortDirection.Ascending);

public enum ModuleTemplateSortField
{
    Name = 0,
    Type = 1,
    Enabled = 2
}

public sealed record ModuleScaffoldingResponse(
    NixModuleScaffoldingDto Module,
    IReadOnlyList<NixTestScaffoldingDto> Tests);

public sealed record NixModuleScaffoldingDto(string Prefix, string Suffix);

public sealed record NixTestScaffoldingDto(string TestName, string Prefix, string Suffix);

public sealed record GetScaffoldingPreviewRequest(ModuleType Type, string? TestNames);

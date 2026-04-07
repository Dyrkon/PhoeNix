using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.Models.Modules;

public sealed record ModuleTemplateListResponse(
    Guid Id,
    string Name,
    bool Enabled,
    ModuleType Type,
    IReadOnlyList<Architecture> SupportedArchitectures);

public sealed record EntryValueDefinitionResponse(
    string Name,
    string Placeholder,
    UserInputType InputType,
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

public sealed record ModuleTemplateResponse(
    Guid Id,
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    bool RequiresSetupBindings,
    IReadOnlyList<EntryValueDefinitionResponse> EditableValueTypes,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ModuleTemplateTestResponse> Tests);

public sealed record ModuleTemplateEntryValueDefinitionModel(
    string Name,
    string Placeholder,
    UserInputType InputType,
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
    string Name,
    string Content,
    IReadOnlyList<string> VariableNames);

public sealed record ModuleEntryValueUpsertModel(
    string Name,
    string Placeholder,
    EntryValueKind Kind,
    string? TextValue,
    int? IntegerUpperValue,
    int? IntegerLowerValue,
    decimal? DecimalUpperValue,
    decimal? DecimalLowerValue,
    string? SelectedValue);

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
    IReadOnlyList<string>? Options);

public sealed record ModuleValueResponse(
    Guid Id,
    bool Enabled,
    List<EntryValueResponse> EditableValues);

public sealed record ModuleValueListResponse(
    Guid Id,
    bool Enabled);

public sealed record ModuleScaffoldingResponse(
    NixModuleScaffoldingDto Module,
    IReadOnlyList<NixTestScaffoldingDto> Tests);

public sealed record NixModuleScaffoldingDto(string Prefix, string Suffix);

public sealed record NixTestScaffoldingDto(string TestName, string Prefix, string Suffix);

public record NixModuleScaffolding(string Prefix, string Suffix);

public record NixTestScaffolding(string Prefix, string Suffix);
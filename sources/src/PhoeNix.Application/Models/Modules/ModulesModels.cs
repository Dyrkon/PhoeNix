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
    int? BindingIndex);

public sealed record ModuleTemplateTestUpsertModel(
    Guid? Id,
    string Name,
    string Content,
    IReadOnlyList<string> VariableNames);

public sealed record EntryValueResponse(
    string Name,
    string Placeholder,
    string Value);

public sealed record ModuleValueResponse(
    Guid Id,
    bool Enabled,
    List<EntryValueResponse> EditableValues);

public sealed record ModuleValueListResponse(
    Guid Id,
    bool Enabled);
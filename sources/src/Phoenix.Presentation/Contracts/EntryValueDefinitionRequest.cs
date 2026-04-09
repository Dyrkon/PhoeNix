using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Enums;

namespace Phoenix.Presentation.Contracts;

public sealed record EntryValueDefinitionRequest(
    string Name,
    string Placeholder,
    EntryBindingKind BindingKind,
    int? BindingIndex);

public sealed record ModuleTemplateTestRequest(
    Guid? Id,
    string Name,
    string Content,
    IReadOnlyList<string> VariableNames);

public sealed record CreateModuleTemplateRequest(
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ModuleTemplateEntryValueDefinitionModel> EditableValueTypes,
    IReadOnlyList<ModuleTemplateTestUpsertModel> Tests);

public sealed record UpdateModuleTemplateRequest(
    string Name,
    bool Enabled,
    ModuleType Type,
    string Content,
    IReadOnlyList<Architecture> SupportedArchitectures,
    IReadOnlyList<ModuleTemplateEntryValueDefinitionModel> EditableValueTypes,
    IReadOnlyList<ModuleTemplateTestUpsertModel> Tests);

public sealed record GetScaffoldingPreviewRequest(ModuleType Type, string? TestNames);
using PhoeNix.Domain.Enums;

namespace PhoeNix.WebAPP.States;

public sealed record TemplateDraftRequiredInput(string Name, string Source);

public sealed record TemplateDraftEntry(
    string Name,
    string Placeholder,
    EntryBindingKind BindingKind,
    EntryValueKind ValueKind,
    int? IntegerMin,
    int? IntegerMax,
    decimal? DecimalMin,
    decimal? DecimalMax,
    List<string> Options,
    List<string> DefaultListItems,
    bool AllowLowerValue,
    string? DefaultValue,
    string? DefaultLowerValue,
    int? BindingIndex);

public sealed record TemplateDraftTest(
    Guid Id,
    string Name,
    string Content,
    List<string> VariableNames);

public class TemplatesState
{
    public bool HasDraft { get; private set; }
    public Guid? TemplateId { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public bool Enabled { get; private set; } = true;
    public ModuleType ModuleType { get; private set; } = ModuleType.Generic;
    public List<Architecture> SelectedArchitectures { get; private set; } = [Architecture.X86Linux];
    public string ModuleContent { get; private set; } = string.Empty;
    public List<TemplateDraftEntry> Entries { get; private set; } = [];
    public List<TemplateDraftTest> Tests { get; private set; } = [];
    public List<TemplateDraftRequiredInput> RequiredInputs { get; private set; } = [];

    public bool IsMatchingDraft(Guid? templateId) => HasDraft && TemplateId == templateId;

    public void SaveDraft(
        Guid? templateId,
        string name,
        bool enabled,
        ModuleType moduleType,
        List<Architecture> architectures,
        string moduleContent,
        List<TemplateDraftEntry> entries,
        List<TemplateDraftTest> tests,
        List<TemplateDraftRequiredInput> requiredInputs)
    {
        HasDraft = true;
        TemplateId = templateId;
        Name = name;
        Enabled = enabled;
        ModuleType = moduleType;
        SelectedArchitectures = architectures;
        ModuleContent = moduleContent;
        Entries = entries;
        Tests = tests;
        RequiredInputs = requiredInputs;
    }

    public void Clear()
    {
        HasDraft = false;
        TemplateId = null;
        Name = string.Empty;
        Enabled = true;
        ModuleType = ModuleType.Generic;
        SelectedArchitectures = [Architecture.X86Linux];
        ModuleContent = string.Empty;
        Entries = [];
        Tests = [];
        RequiredInputs = [];
    }
}

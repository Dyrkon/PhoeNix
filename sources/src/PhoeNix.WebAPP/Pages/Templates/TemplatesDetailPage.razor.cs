using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.Contracts.Modules;
using PhoeNix.WebAPP.Helpers;

namespace PhoeNix.WebAPP.Pages.Templates;

public partial class TemplatesDetailPage : ComponentBase
{
    [Inject] private IModulesApiClient ModulesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid TemplateId { get; set; }

    private ModuleTemplateResponse? _template;
    private ModuleScaffoldingResponse? _scaffolding;
    private bool _isLoading = true;
    private ViewMode _viewMode = ViewMode.Overview;
    private bool _entriesExpanded = true;
    private bool _testsExpanded = true;

    private bool _moduleScaffoldingExpanded = false;
    private HashSet<Guid> _testScaffoldingExpanded = new();

    private MarkupString _modulePrefix;
    private MarkupString _moduleContent;
    private MarkupString _moduleSuffix;
    private Dictionary<Guid, (MarkupString Prefix, MarkupString Content, MarkupString Suffix)> _testParts = new();

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;

        var templateTask = ModulesApiClient.GetModuleTemplateByIdAsync(TemplateId);
        var scaffoldingTask = ModulesApiClient.GetModuleScaffoldingAsync(TemplateId);

        await Task.WhenAll(templateTask, scaffoldingTask);

        var templateResponse = templateTask.Result;
        var scaffoldingResponse = scaffoldingTask.Result;

        if (templateResponse.IsFailure || templateResponse.Value is null)
        {
            Snackbar.Add("Failed to load module template detail.", Severity.Error);
            _template = null;
            _scaffolding = null;
            _isLoading = false;
            return;
        }

        _template = templateResponse.Value;
        _scaffolding = scaffoldingResponse.IsSuccess ? scaffoldingResponse.Value : null;
        _moduleScaffoldingExpanded = false;
        _testScaffoldingExpanded = new HashSet<Guid>();

        ComputeHighlightedParts();

        _isLoading = false;
    }

    private void ComputeHighlightedParts()
    {
        if (_template is null)
            return;

        var entryPairs = _template.EditableValueTypes
            .Where(e => !string.IsNullOrEmpty(e.Name))
            .Select(e => (e.Name, e.Placeholder))
            .ToList();

        if (_scaffolding is null)
        {
            _modulePrefix = new MarkupString(string.Empty);
            _moduleContent = new MarkupString(NixCodeHighlighter.HighlightEntryValues(
                NixCodeFormatter.Format(_template.Content), entryPairs));
            _moduleSuffix = new MarkupString(string.Empty);
        }
        else
        {
            var (formattedPrefix, prefixEndIndent) = NixCodeFormatter.Format(_scaffolding.Module.Prefix, 0);
            var (formattedContent, contentEndIndent) = NixCodeFormatter.Format(_template.Content, prefixEndIndent);
            var (formattedSuffix, _) = NixCodeFormatter.Format(_scaffolding.Module.Suffix, contentEndIndent);

            _modulePrefix = new MarkupString(NixCodeHighlighter.WrapAsScaffolding(formattedPrefix));
            _moduleContent = new MarkupString(NixCodeHighlighter.HighlightEntryValues(formattedContent, entryPairs));
            _moduleSuffix = new MarkupString(NixCodeHighlighter.WrapAsScaffolding(formattedSuffix));
        }

        _testParts = new Dictionary<Guid, (MarkupString Prefix, MarkupString Content, MarkupString Suffix)>();

        foreach (var test in _template.Tests)
        {
            var testScaffolding = _scaffolding?.Tests.FirstOrDefault(t => t.TestName == test.Name);

            if (testScaffolding is null)
            {
                var formatted = NixCodeFormatter.Format(test.Content);
                var highlighted = NixCodeHighlighter.HighlightVariables(formatted, test.VariableNames);
                _testParts[test.Id] = (new MarkupString(string.Empty), new MarkupString(highlighted),
                    new MarkupString(string.Empty));
            }
            else
            {
                var (formattedPrefix, prefixEndIndent) = NixCodeFormatter.Format(testScaffolding.Prefix, 0);
                var (formattedContent, contentEndIndent) = NixCodeFormatter.Format(test.Content, prefixEndIndent);
                var (formattedSuffix, _) = NixCodeFormatter.Format(testScaffolding.Suffix, contentEndIndent);

                _testParts[test.Id] = (
                    new MarkupString(NixCodeHighlighter.WrapAsScaffolding(formattedPrefix)),
                    new MarkupString(NixCodeHighlighter.HighlightVariables(formattedContent, test.VariableNames)),
                    new MarkupString(NixCodeHighlighter.WrapAsScaffolding(formattedSuffix))
                );
            }
        }
    }

    private void ToggleModuleScaffolding()
    {
        _moduleScaffoldingExpanded = !_moduleScaffoldingExpanded;
    }

    private void ToggleTestScaffolding(Guid testId)
    {
        if (!_testScaffoldingExpanded.Add(testId))
            _testScaffoldingExpanded.Remove(testId);
    }

    private bool IsTestScaffoldingExpanded(Guid testId)
    {
        return _testScaffoldingExpanded.Contains(testId);
    }

    private bool HasModuleScaffolding =>
        _scaffolding is not null &&
        (!string.IsNullOrEmpty(_scaffolding.Module.Prefix) || !string.IsNullOrEmpty(_scaffolding.Module.Suffix));

    private bool HasTestScaffolding(ModuleTemplateTestResponse test)
    {
        return _scaffolding?.Tests.Any(t => t.TestName == test.Name) == true;
    }

    private static string GetDefaultValueDisplay(EntryValueDefinitionResponse entry)
    {
        if (entry.ValueKind != EntryValueKind.List)
            return entry.DefaultValue ?? "-";

        if (string.IsNullOrEmpty(entry.DefaultValue))
            return "-";

        try
        {
            var items = JsonSerializer.Deserialize<List<string>>(entry.DefaultValue) ?? [];
            return items.Count == 0 ? "(empty list)" : string.Join(", ", items);
        }
        catch
        {
            return entry.DefaultValue;
        }
    }

    private enum ViewMode
    {
        Overview,
        Code
    }
}
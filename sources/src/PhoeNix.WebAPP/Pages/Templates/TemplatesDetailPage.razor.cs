using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
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
        _isLoading = false;
    }

    private IEnumerable<(string Name, string Placeholder)> GetEntryValuePairs()
    {
        if (_template is null)
            return [];

        return _template.EditableValueTypes
            .Where(e => !string.IsNullOrEmpty(e.Name))
            .Select(e => (e.Name, e.Placeholder));
    }

    private MarkupString GetHighlightedModuleContent()
    {
        if (_template is null || string.IsNullOrWhiteSpace(_template.Content))
            return new MarkupString(string.Empty);

        var entryPairs = GetEntryValuePairs().ToList();

        if (_scaffolding is null)
        {
            var formatted = NixCodeFormatter.Format(_template.Content);
            var highlighted = NixCodeHighlighter.HighlightEntryValues(formatted, entryPairs);
            return new MarkupString(highlighted);
        }

        var (formattedPrefix, prefixEndIndent) = NixCodeFormatter.Format(_scaffolding.Module.Prefix, 0);
        var (formattedContent, contentEndIndent) = NixCodeFormatter.Format(_template.Content, prefixEndIndent);
        var (formattedSuffix, _) = NixCodeFormatter.Format(_scaffolding.Module.Suffix, contentEndIndent);

        var styledPrefix = NixCodeHighlighter.WrapAsScaffolding(formattedPrefix);
        var styledContent = NixCodeHighlighter.HighlightEntryValues(formattedContent, entryPairs);
        var styledSuffix = NixCodeHighlighter.WrapAsScaffolding(formattedSuffix);

        var combined = styledPrefix + "\n" + styledContent + "\n" + styledSuffix;
        return new MarkupString(combined);
    }

    private MarkupString GetHighlightedTestContent(ModuleTemplateTestResponse test)
    {
        if (string.IsNullOrWhiteSpace(test.Content))
            return new MarkupString(string.Empty);

        var testScaffolding = _scaffolding?.Tests.FirstOrDefault(t => t.TestName == test.Name);

        if (testScaffolding is null)
        {
            var formatted = NixCodeFormatter.Format(test.Content);
            var highlighted = NixCodeHighlighter.HighlightVariables(formatted, test.VariableNames);
            return new MarkupString(highlighted);
        }

        var (formattedPrefix, prefixEndIndent) = NixCodeFormatter.Format(testScaffolding.Prefix, 0);
        var (formattedContent, contentEndIndent) = NixCodeFormatter.Format(test.Content, prefixEndIndent);
        var (formattedSuffix, _) = NixCodeFormatter.Format(testScaffolding.Suffix, contentEndIndent);

        var styledPrefix = NixCodeHighlighter.WrapAsScaffolding(formattedPrefix);
        var styledContent = NixCodeHighlighter.HighlightVariables(formattedContent, test.VariableNames);
        var styledSuffix = NixCodeHighlighter.WrapAsScaffolding(formattedSuffix);

        var combined = styledPrefix + "\n" + styledContent + "\n" + styledSuffix;
        return new MarkupString(combined);
    }

    private enum ViewMode
    {
        Overview,
        Code
    }
}
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.Components.Templates;
using PhoeNix.WebAPP.Extensions;

namespace PhoeNix.WebAPP.Pages.Templates;

public partial class TemplateCreatorPage : ComponentBase
{
    [Inject] private IModulesApiClient ModulesApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;

    [Parameter] public Guid? TemplateId { get; set; }

    private string _name = string.Empty;
    private bool _enabled = true;
    private ModuleType _moduleType = ModuleType.Generic;
    private List<Architecture> _selectedArchitectures = [Architecture.X86Linux];
    private string _moduleContent = string.Empty;

    private readonly List<EntryEditorModel> _entries = [];
    private readonly List<TestEditorModel> _tests = [];
    private TestEditorModel? _selectedTest;

    private readonly List<string> _validationErrors = [];
    private bool _isSubmitting;

    private NixModuleScaffoldingDto? _moduleScaffolding;
    private readonly Dictionary<string, NixTestScaffoldingDto> _testScaffoldings = new();
    private bool _isLoadingScaffolding;

    private bool _moduleScaffoldingExpanded = false;
    private readonly HashSet<Guid> _testScaffoldingExpanded = new();

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

    protected override async Task OnInitializedAsync()
    {
        if (TemplateId.HasValue)
        {
            var result = await ModulesApiClient.GetModuleTemplateByIdAsync(TemplateId.Value);

            if (result.IsSuccess && result.Value is not null)
            {
                var t = result.Value;
                _name = t.Name;
                _enabled = t.Enabled;
                _moduleType = t.Type;
                _selectedArchitectures = t.SupportedArchitectures.ToList();
                _moduleContent = t.Content;

                _entries.Clear();
                foreach (var e in t.EditableValueTypes)
                    _entries.Add(new EntryEditorModel
                    {
                        Name = e.Name,
                        Placeholder = e.Placeholder,
                        BindingKind = e.BindingKind,
                        ValueKind = e.ValueKind,
                        DefaultValue = e.ValueKind == EntryValueKind.List ? null : e.DefaultValue,
                        DefaultListItems = e.ValueKind == EntryValueKind.List && e.DefaultValue is not null
                            ? JsonSerializer.Deserialize<List<string>>(e.DefaultValue) ?? []
                            : [],
                        DefaultLowerValue = e.DefaultLowerValue,
                        IntegerMin = e.IntegerMin,
                        IntegerMax = e.IntegerMax,
                        DecimalMin = e.DecimalMin,
                        DecimalMax = e.DecimalMax,
                        AllowLowerValue = e.AllowLowerValue,
                        Options = e.Options is not null ? [..e.Options] : [],
                        BindingIndex = e.BindingIndex
                    });

                _tests.Clear();
                foreach (var test in t.Tests)
                    _tests.Add(new TestEditorModel
                    {
                        Id = test.Id,
                        Name = test.Name,
                        Content = test.Content,
                        VariableNames = test.VariableNames.ToList()
                    });

                _selectedTest = _tests.FirstOrDefault();
            }
            else
            {
                Snackbar.Add("Failed to load template for editing.", Severity.Error);
            }
        }

        await FetchScaffoldingAsync();
    }

    private void OnArchitecturesChanged(IEnumerable<Architecture> values)
    {
        _selectedArchitectures = values.ToList();
    }

    private async Task OnModuleTypeChanged(ModuleType newType)
    {
        _moduleType = newType;
        await FetchScaffoldingAsync();
    }

    private async Task FetchScaffoldingAsync()
    {
        _isLoadingScaffolding = true;

        try
        {
            var testNames = _tests.Select(t => t.Name).Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
            var result = await ModulesApiClient.GetScaffoldingPreviewAsync(_moduleType, testNames);

            if (result.IsSuccess && result.Value is not null)
            {
                _moduleScaffolding = result.Value.Module;
                _testScaffoldings.Clear();
                foreach (var testScaffolding in result.Value.Tests)
                    _testScaffoldings[testScaffolding.TestName] = testScaffolding;
            }
        }
        finally
        {
            _isLoadingScaffolding = false;
        }
    }

    private async Task OpenAddEntryDialogAsync()
    {
        var parameters = new DialogParameters<EntryDefinitionDialog>
        {
            { x => x.Model, new EntryDefinitionDialog.EntryDefinitionModel() },
            { x => x.IsEditMode, false },
            { x => x.ExistingNames, _entries.Select(e => e.Name).ToList() },
            { x => x.ExistingPlaceholders, _entries.Select(e => e.Placeholder).ToList() }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseOnEscapeKey = true
        };

        var dialog = await DialogService.ShowAsync<EntryDefinitionDialog>("Add Entry Definition", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: EntryDefinitionDialog.EntryDefinitionModel model })
            _entries.Add(MapToEditorModel(model));
    }

    private async Task OpenEditEntryDialogAsync(EntryEditorModel entry)
    {
        var dialogModel = MapToDialogModel(entry);

        var parameters = new DialogParameters<EntryDefinitionDialog>
        {
            { x => x.Model, dialogModel },
            { x => x.IsEditMode, true },
            { x => x.ExistingNames, _entries.Where(e => e != entry).Select(e => e.Name).ToList() },
            { x => x.ExistingPlaceholders, _entries.Where(e => e != entry).Select(e => e.Placeholder).ToList() }
        };

        var options = new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true,
            CloseOnEscapeKey = true
        };

        var dialog = await DialogService.ShowAsync<EntryDefinitionDialog>("Edit Entry Definition", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: EntryDefinitionDialog.EntryDefinitionModel model })
        {
            var index = _entries.IndexOf(entry);
            if (index >= 0) _entries[index] = MapToEditorModel(model);
        }
    }

    private void RemoveEntry(EntryEditorModel entry)
    {
        _entries.Remove(entry);
    }

    private async Task AddTestAsync()
    {
        var test = new TestEditorModel
        {
            Id = Guid.NewGuid(),
            Name = $"Test {_tests.Count + 1}",
            Content = string.Empty,
            VariableNames = []
        };

        _tests.Add(test);
        _selectedTest = test;
        await FetchScaffoldingAsync();
    }

    private async Task RemoveTestAsync(TestEditorModel test)
    {
        _tests.Remove(test);

        if (_selectedTest == test) _selectedTest = _tests.FirstOrDefault();

        await FetchScaffoldingAsync();
    }

    private async Task OnTestNameChangedAsync(TestEditorModel test, string newName)
    {
        test.Name = newName;
        await FetchScaffoldingAsync();
    }

    private void OnSelectedVariablesChanged(IEnumerable<string> selectedPlaceholders)
    {
        if (_selectedTest is null) return;
        _selectedTest.VariableNames = selectedPlaceholders.ToList();
    }

    private IReadOnlyList<string> GetAvailablePlaceholders()
    {
        return _entries.Select(e => e.Placeholder).Where(p => !string.IsNullOrWhiteSpace(p)).ToList();
    }

    private NixTestScaffoldingDto? GetTestScaffolding(string testName)
    {
        return _testScaffoldings.GetValueOrDefault(testName);
    }

    private void ValidateForm()
    {
        _validationErrors.Clear();

        if (string.IsNullOrWhiteSpace(_name)) _validationErrors.Add("Name is required.");

        if (!_selectedArchitectures.Any()) _validationErrors.Add("At least one architecture must be selected.");

        if (string.IsNullOrWhiteSpace(_moduleContent)) _validationErrors.Add("Module content is required.");

        foreach (var entry in _entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Placeholder))
                continue;

            if (!_moduleContent.Contains(entry.Placeholder))
                _validationErrors.Add($"Placeholder '{entry.Placeholder}' not found in module content.");
        }

        var duplicateNames = _entries
            .GroupBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var name in duplicateNames) _validationErrors.Add($"Duplicate entry name: '{name}'.");

        var duplicatePlaceholders = _entries
            .GroupBy(e => e.Placeholder, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var placeholder in duplicatePlaceholders)
            _validationErrors.Add($"Duplicate placeholder: '{placeholder}'.");

        foreach (var test in _tests)
        {
            if (string.IsNullOrWhiteSpace(test.Name)) _validationErrors.Add("All tests must have a name.");

            if (string.IsNullOrWhiteSpace(test.Content))
                _validationErrors.Add($"Test '{test.Name}' must have content.");

            foreach (var variable in test.VariableNames)
                if (!string.IsNullOrWhiteSpace(test.Content) && !test.Content.Contains(variable))
                    _validationErrors.Add($"Variable '{variable}' not found in test '{test.Name}' content.");
        }

        var duplicateTestNames = _tests
            .Where(t => !string.IsNullOrWhiteSpace(t.Name))
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        foreach (var name in duplicateTestNames) _validationErrors.Add($"Duplicate test name: '{name}'.");

        if (_validationErrors.Count == 0)
            Snackbar.Add("Validation passed.", Severity.Success);
        else
            Snackbar.Add($"Validation failed with {_validationErrors.Count} error(s).", Severity.Warning);
    }

    private async Task SubmitAsync()
    {
        ValidateForm();

        if (_validationErrors.Count > 0) return;

        _isSubmitting = true;

        try
        {
            if (TemplateId.HasValue)
            {
                var request = new UpdateModuleTemplateRequest(
                    _name.Trim(),
                    _enabled,
                    _moduleType,
                    _moduleContent,
                    _selectedArchitectures.ToList(),
                    _entries.Select(MapToApiModel).ToList(),
                    _tests.Select(MapTestToApiModel).ToList());

                var result = await ModulesApiClient.UpdateModuleTemplateAsync(TemplateId.Value, request);

                if (result.IsFailure)
                {
                    Snackbar.Add($"Failed to update template: {result.Error?.Description ?? "Unknown error"}",
                        Severity.Error);
                    return;
                }

                Snackbar.Add("Template updated successfully.", Severity.Success);
                NavigationManager.NavigateToTemplatesDetail(TemplateId.Value);
            }
            else
            {
                var request = new CreateModuleTemplateRequest(
                    _name.Trim(),
                    _enabled,
                    _moduleType,
                    _moduleContent,
                    _selectedArchitectures.ToList(),
                    _entries.Select(MapToApiModel).ToList(),
                    _tests.Select(MapTestToApiModel).ToList());

                var result = await ModulesApiClient.CreateModuleTemplateAsync(request);

                if (result.IsFailure)
                {
                    Snackbar.Add($"Failed to create template: {result.Error?.Description ?? "Unknown error"}",
                        Severity.Error);
                    return;
                }

                Snackbar.Add("Template created successfully.", Severity.Success);
                NavigationManager.NavigateToTemplates();
            }
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private static EntryEditorModel MapToEditorModel(EntryDefinitionDialog.EntryDefinitionModel model)
    {
        return new EntryEditorModel
        {
            Name = model.Name,
            Placeholder = model.Placeholder,
            BindingKind = model.BindingKind,
            ValueKind = model.ValueKind,
            IntegerMin = model.IntegerMin,
            IntegerMax = model.IntegerMax,
            DecimalMin = model.DecimalMin,
            DecimalMax = model.DecimalMax,
            Options = [..model.Options],
            DefaultListItems = [..model.DefaultListItems],
            AllowLowerValue = model.AllowLowerValue,
            DefaultValue = model.DefaultValue,
            DefaultLowerValue = model.DefaultLowerValue,
            BindingIndex = model.BindingIndex
        };
    }

    private static EntryDefinitionDialog.EntryDefinitionModel MapToDialogModel(EntryEditorModel model)
    {
        return new EntryDefinitionDialog.EntryDefinitionModel
        {
            Name = model.Name,
            Placeholder = model.Placeholder,
            BindingKind = model.BindingKind,
            ValueKind = model.ValueKind,
            IntegerMin = model.IntegerMin,
            IntegerMax = model.IntegerMax,
            DecimalMin = model.DecimalMin,
            DecimalMax = model.DecimalMax,
            Options = [..model.Options],
            DefaultListItems = [..model.DefaultListItems],
            AllowLowerValue = model.AllowLowerValue,
            DefaultValue = model.DefaultValue,
            DefaultLowerValue = model.DefaultLowerValue,
            BindingIndex = model.BindingIndex
        };
    }

    private static ModuleTemplateEntryValueDefinitionModel MapToApiModel(EntryEditorModel model)
    {
        var defaultValue = model.ValueKind == EntryValueKind.List
            ? JsonSerializer.Serialize(model.DefaultListItems)
            : model.DefaultValue;

        return new ModuleTemplateEntryValueDefinitionModel(
            model.Name,
            model.Placeholder,
            model.BindingKind,
            model.ValueKind,
            defaultValue,
            model.DefaultLowerValue,
            model.IntegerMin,
            model.IntegerMax,
            model.DecimalMin,
            model.DecimalMax,
            model.AllowLowerValue,
            model.Options.Count > 0 ? model.Options : null,
            model.BindingIndex);
    }

    private static ModuleTemplateTestUpsertModel MapTestToApiModel(TestEditorModel model)
    {
        return new ModuleTemplateTestUpsertModel(
            model.Id,
            model.Name,
            model.Content,
            model.VariableNames);
    }

    private sealed class EntryEditorModel
    {
        public string Name { get; set; } = string.Empty;
        public string Placeholder { get; set; } = string.Empty;
        public EntryBindingKind BindingKind { get; set; } = EntryBindingKind.UserProvided;
        public EntryValueKind ValueKind { get; set; } = EntryValueKind.Text;
        public int? IntegerMin { get; set; }
        public int? IntegerMax { get; set; }
        public decimal? DecimalMin { get; set; }
        public decimal? DecimalMax { get; set; }
        public List<string> Options { get; set; } = [];
        public List<string> DefaultListItems { get; set; } = [];
        public bool AllowLowerValue { get; set; }
        public string? DefaultValue { get; set; }
        public string? DefaultLowerValue { get; set; }
        public int? BindingIndex { get; set; }
    }

    private sealed class TestEditorModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> VariableNames { get; set; } = [];
    }
}
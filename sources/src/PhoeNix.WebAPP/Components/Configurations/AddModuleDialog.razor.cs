using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Modules;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Models;
using static PhoeNix.WebAPP.Components.Configurations.ModuleEntryEditorHelpers;
using UpdateSharedRequest = PhoeNix.Contracts.Configurations.UpdateConfigurationModuleRequest;
using UpdateSystemRequest = PhoeNix.Contracts.Configurations.UpdateConfigurationSystemModuleRequest;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class AddModuleDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private IModulesApiClient ModulesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid ConfigurationId { get; set; }
    [Parameter] public Guid? SystemId { get; set; }

    private MudForm? _form;
    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _loadingEntries;
    private Guid? _selectedTemplateId;
    private bool _enabled = true;
    private string _newListItem = string.Empty;
    private List<ModuleTemplateListResponse> _templates = [];
    private List<EntryEditModel> _entryModels = [];

    protected override async Task OnInitializedAsync()
    {
        var result = await ModulesApiClient.GetModuleTemplatesAsync(
            new ListModuleTemplatesRequest(PageSize: 100));

        if (result.IsSuccess && result.Value is not null)
            _templates = result.Value.Items.ToList();
        else
            Snackbar.Add("Failed to load module templates.", Severity.Warning);

        _isLoading = false;
    }

    private async Task OnTemplateSelectedAsync(Guid? templateId)
    {
        _selectedTemplateId = templateId;
        _entryModels.Clear();

        if (templateId is null)
            return;

        _loadingEntries = true;
        StateHasChanged();

        var result = await ModulesApiClient.GetModuleTemplateByIdAsync(templateId.Value);

        if (result.IsSuccess && result.Value is not null)
            foreach (var def in result.Value.EditableValueTypes)
                _entryModels.Add(BuildEntryModel(def));

        _loadingEntries = false;
    }

    private async Task SubmitAsync()
    {
        if (_form is null || _selectedTemplateId is null)
            return;

        await _form.Validate();
        if (!_form.IsValid)
            return;

        _isSubmitting = true;

        try
        {
            ApiResult<ModuleValueResponse> addResult;

            if (SystemId.HasValue)
                addResult = await ConfigurationsApiClient.AddConfigurationSystemModuleAsync(
                    ConfigurationId,
                    SystemId.Value,
                    new CreateConfigurationSystemModuleRequest(_selectedTemplateId.Value, _enabled));
            else
                addResult = await ConfigurationsApiClient.AddConfigurationModuleAsync(
                    ConfigurationId,
                    new CreateConfigurationModuleRequest(_selectedTemplateId.Value, _enabled));

            if (addResult.IsFailure || addResult.Value is null)
            {
                Snackbar.Add("Failed to add module.", Severity.Error);
                return;
            }

            if (_entryModels.Count > 0)
            {
                var entries = _entryModels.Select(ToUpsertModel).ToList();
                var moduleValueId = addResult.Value.Id;

                var updateResult = SystemId.HasValue
                    ? await ConfigurationsApiClient.UpdateConfigurationSystemModuleAsync(
                        ConfigurationId, SystemId.Value, moduleValueId,
                        new UpdateSystemRequest(_enabled, entries))
                    : await ConfigurationsApiClient.UpdateConfigurationModuleAsync(
                        ConfigurationId, moduleValueId,
                        new UpdateSharedRequest(_enabled, entries));

                if (updateResult.IsFailure)
                {
                    Snackbar.Add("Module added but entry values could not be saved.", Severity.Warning);
                    MudDialog.Close(DialogResult.Ok(true));
                    return;
                }
            }

            Snackbar.Add("Module added.", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void AddListItem(EntryEditModel entry)
    {
        if (string.IsNullOrWhiteSpace(_newListItem))
            return;

        entry.ListItems.Add(_newListItem.Trim());
        _newListItem = string.Empty;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }
}
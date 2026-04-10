using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.ApiClient.Models;
using static PhoeNix.WebAPP.Components.Configurations.ModuleEntryEditorHelpers;
using UpdateSharedModuleRequest = PhoeNix.WebAPP.ApiClient.Contracts.UpdateConfigurationModuleRequest;
using UpdateSystemModuleRequest = PhoeNix.WebAPP.ApiClient.Contracts.UpdateConfigurationSystemModuleRequest;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class EditModuleValuesDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IModulesApiClient ModulesApiClient { get; set; } = null!;
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid ConfigurationId { get; set; }
    [Parameter] public Guid? SystemId { get; set; }
    [Parameter] public ConfiguredModuleResponse Module { get; set; } = null!;

    private bool _isLoading = true;
    private bool _isSubmitting;
    private bool _enabled;
    private string _newListItem = string.Empty;
    private List<EntryEditModel> _entryModels = [];

    protected override async Task OnInitializedAsync()
    {
        _enabled = Module.Enabled;

        var templateResult = await ModulesApiClient.GetModuleTemplateByIdAsync(Module.ModuleTemplateId);

        if (templateResult.IsFailure || templateResult.Value is null)
        {
            Snackbar.Add("Failed to load module template.", Severity.Warning);
            _isLoading = false;
            return;
        }

        foreach (var def in templateResult.Value.EditableValueTypes)
        {
            var current = Module.Entries.FirstOrDefault(e => e.Name == def.Name);
            _entryModels.Add(BuildEntryModel(def, current));
        }

        _isLoading = false;
    }

    private async Task SubmitAsync()
    {
        _isSubmitting = true;

        try
        {
            var entries = _entryModels.Select(ToUpsertModel).ToList();

            ApiResult result;

            if (SystemId.HasValue)
            {
                result = await ConfigurationsApiClient.UpdateConfigurationSystemModuleAsync(
                    ConfigurationId,
                    SystemId.Value,
                    Module.ModuleValueId,
                    new UpdateSystemModuleRequest(_enabled, entries));
            }
            else
            {
                result = await ConfigurationsApiClient.UpdateConfigurationModuleAsync(
                    ConfigurationId,
                    Module.ModuleValueId,
                    new UpdateSharedModuleRequest(_enabled, entries));
            }

            if (result.IsFailure)
            {
                Snackbar.Add("Failed to save module values.", Severity.Error);
                return;
            }

            Snackbar.Add("Module updated.", Severity.Success);
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

    private void Cancel() => MudDialog.Cancel();
}

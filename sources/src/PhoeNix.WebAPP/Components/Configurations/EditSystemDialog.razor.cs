using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.Contracts.Configurations;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class EditSystemDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid ConfigurationId { get; set; }
    [Parameter] public Guid SystemId { get; set; }
    [Parameter] public string CurrentName { get; set; } = string.Empty;

    private MudForm? _form;
    private bool _isSubmitting;
    private string _name = string.Empty;

    protected override void OnParametersSet()
    {
        _name = CurrentName;
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
            return;

        await _form.Validate();
        if (!_form.IsValid)
            return;

        _isSubmitting = true;

        try
        {
            var result = await ConfigurationsApiClient.UpdateConfigurationSystemAsync(
                ConfigurationId,
                SystemId,
                new UpdateConfigurationSystemRequest(_name));

            if (result.IsFailure)
            {
                Snackbar.Add("Failed to rename system.", Severity.Error);
                return;
            }

            Snackbar.Add("System renamed.", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();
}

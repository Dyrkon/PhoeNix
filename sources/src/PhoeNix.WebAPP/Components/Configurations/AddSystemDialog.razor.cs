using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class AddSystemDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid ConfigurationId { get; set; }

    private MudForm? _form;
    private bool _isSubmitting;
    private string _name = string.Empty;
    private Architecture _architecture = Architecture.X86Linux;

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
            var result = await ConfigurationsApiClient.AddConfigurationSystemAsync(
                ConfigurationId,
                new CreateConfigurationSystemRequest(_name, _architecture));

            if (result.IsFailure || result.Value is null)
            {
                Snackbar.Add("Failed to add system.", Severity.Error);
                return;
            }

            Snackbar.Add("System added.", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();
}

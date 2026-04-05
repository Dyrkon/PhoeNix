using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class CreateConfigurationDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private MudForm? _form;
    private bool _isSubmitting;

    private CreateConfigurationFormModel _model = new()
    {
        Title = string.Empty,
        Description = string.Empty
    };

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
            var request = new CreateConfigurationRequest(
                _model.Title.Trim(),
                _model.Description.Trim());

            var result = await ConfigurationsApiClient.CreateConfigurationAsync(request);

            if (result.IsFailure)
            {
                Snackbar.Add("Failed to create configuration.", Severity.Error);
                return;
            }

            Snackbar.Add("Configuration created.", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private sealed class CreateConfigurationFormModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
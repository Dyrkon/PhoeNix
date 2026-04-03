using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class CreateMachineDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private MudForm? _form;
    private bool _isSubmitting;

    private readonly CreateMachineFormModel _model = new()
    {
        Title = string.Empty,
        Enabled = true,
        MacAddress = string.Empty,
        Architecture = Enum.GetValues<Architecture>().First(),
        InstallDiskSelectionPreference = InstallDiskSelectionPreference.Biggest
    };

    private async Task SubmitAsync()
    {
        if (_form is null)
            return;

        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        _isSubmitting = true;

        try
        {
            var request = new CreateMachineRequest(
                _model.Title.Trim(),
                _model.Enabled,
                NormalizeMacAddress(_model.MacAddress),
                _model.Architecture,
                _model.InstallDiskSelectionPreference);

            var result = await MachinesApiClient.CreateMachineAsync(request);

            if (result.IsFailure)
            {
                Snackbar.Add("Failed to create machine.", Severity.Error);
                return;
            }

            Snackbar.Add("Machine created.", Severity.Success);
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

    private static string NormalizeMacAddress(string value)
    {
        return value.Trim().Replace("-", ":");
    }

    private sealed class CreateMachineFormModel
    {
        public string Title { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public Architecture Architecture { get; set; }
        public InstallDiskSelectionPreference InstallDiskSelectionPreference { get; set; }
    }
}
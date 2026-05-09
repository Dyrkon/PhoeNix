using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class EditMachineDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid MachineId { get; set; }
    [Parameter] public MachineDetailResponse Machine { get; set; } = null!;

    private MudForm? _form;
    private bool _isSubmitting;
    private string? _errorMessage;

    private EditMachineFormModel _model = new();

    protected override void OnParametersSet()
    {
        _model = new EditMachineFormModel
        {
            Title = Machine.Title,
            Enabled = Machine.Enabled,
            MacAddress = Machine.MacAddress,
            Architecture = Machine.Architecture,
            InstallDiskSelectionPreference = Machine.InstallDiskSelectionPreference
        };
    }

    private async Task SubmitAsync()
    {
        if (_form is null)
            return;

        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        _isSubmitting = true;
        _errorMessage = null;

        try
        {
            var request = new UpdateMachineRequest(
                _model.Title.Trim(),
                _model.Enabled,
                NormalizeMacAddress(_model.MacAddress),
                _model.Architecture,
                _model.InstallDiskSelectionPreference);

            var result = await MachinesApiClient.UpdateMachineAsync(MachineId, request);

            if (result.IsFailure)
            {
                _errorMessage = result.Error?.Description ?? result.Error?.Code ?? "Failed to update machine.";
                return;
            }

            Snackbar.Add("Machine updated.", Severity.Success);
            MudDialog.Close(DialogResult.Ok(true));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();

    private static string NormalizeMacAddress(string value) =>
        value.Trim().Replace("-", ":");

    private sealed class EditMachineFormModel
    {
        public string Title { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string MacAddress { get; set; } = string.Empty;
        public Architecture Architecture { get; set; }
        public InstallDiskSelectionPreference InstallDiskSelectionPreference { get; set; }
    }
}

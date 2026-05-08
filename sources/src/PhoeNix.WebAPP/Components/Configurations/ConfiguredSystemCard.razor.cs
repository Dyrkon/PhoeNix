using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Validation;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfiguredSystemCard : ComponentBase
{
    [Inject] private ISystemsApiClient SystemsApiClient { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] [EditorRequired] public ConfiguredSystemResponse System { get; set; } = null!;
    [Parameter] [EditorRequired] public Guid ConfigurationId { get; set; }
    [Parameter] public IReadOnlyList<Architecture> ConfigurationArchitectures { get; set; } = [];
    [Parameter] public EventCallback OnModuleUpdated { get; set; }

    private MudExpansionPanels? _modulePanels;
    private bool _modulesExpanded;

    private SystemValidationStatusResponse? _validationStatus;
    private bool _isValidating;
    private CancellationTokenSource? _pollCts;

    protected override async Task OnInitializedAsync()
    {
        var result = await SystemsApiClient.GetSystemValidationStatusAsync(ConfigurationId, System.Id);
        if (result.IsSuccess && result.Value is not null)
            _validationStatus = result.Value;
    }

    private async Task ToggleModulesAsync()
    {
        if (_modulePanels is null) return;
        if (_modulesExpanded)
            await _modulePanels.CollapseAllAsync();
        else
            await _modulePanels.ExpandAllAsync();
        _modulesExpanded = !_modulesExpanded;
    }

    private async Task ScheduleValidationAsync()
    {
        if (_isValidating) return;

        _isValidating = true;
        _pollCts?.Cancel();
        _pollCts = new CancellationTokenSource();

        var result = await SystemsApiClient.ScheduleSystemValidationAsync(ConfigurationId, System.Id);
        if (result.IsFailure)
        {
            Snackbar.Add($"Failed to schedule validation: {result.Error?.Description}", Severity.Error);
            _isValidating = false;
            return;
        }

        _validationStatus = new SystemValidationStatusResponse("Queued", null, null, null);
        StateHasChanged();

        _ = PollStatusAsync(_pollCts.Token);
    }

    private async Task PollStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(3000, cancellationToken);

                var result = await SystemsApiClient.GetSystemValidationStatusAsync(
                    ConfigurationId, System.Id, cancellationToken);

                if (result.IsSuccess && result.Value is not null)
                {
                    _validationStatus = result.Value;
                    await InvokeAsync(StateHasChanged);

                    if (_validationStatus.State is "Succeeded" or "Failed")
                        break;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isValidating = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OpenErrorDialogAsync()
    {
        if (_validationStatus is null) return;

        var parameters = new DialogParameters<ValidationErrorDialog>
        {
            { x => x.ErrorCode, _validationStatus.ErrorCode },
            { x => x.ErrorMessage, _validationStatus.ErrorMessage },
            { x => x.TestResults, null }
        };

        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseOnEscapeKey = true };
        await DialogService.ShowAsync<ValidationErrorDialog>("Validation Failed", parameters, options);
    }
}

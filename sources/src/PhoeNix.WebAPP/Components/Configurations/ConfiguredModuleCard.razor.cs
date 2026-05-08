using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Validation;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfiguredModuleCard : ComponentBase
{
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private IModulesApiClient ModulesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] [EditorRequired] public ConfiguredModuleResponse Module { get; set; } = null!;
    [Parameter] [EditorRequired] public Guid ConfigurationId { get; set; }
    [Parameter] public Guid? SystemId { get; set; }
    [Parameter] public IReadOnlyList<Architecture> SupportedArchitectures { get; set; } = [];
    [Parameter] public EventCallback OnModuleUpdated { get; set; }

    private Architecture? _selectedArch;
    private ModuleValidationStatusResponse? _validationStatus;
    private bool _isValidating;
    private CancellationTokenSource? _pollCts;

    protected override async Task OnInitializedAsync()
    {
        _selectedArch = SupportedArchitectures.FirstOrDefault();
        if (_selectedArch.HasValue)
            await LoadStatusAsync(_selectedArch.Value);
    }

    private async Task OnArchitectureChangedAsync(Architecture? arch)
    {
        _selectedArch = arch;
        _validationStatus = null;
        _pollCts?.Cancel();
        _isValidating = false;
        if (arch.HasValue)
            await LoadStatusAsync(arch.Value);
    }

    private async Task LoadStatusAsync(Architecture arch)
    {
        var result = await ModulesApiClient.GetModuleValidationStatusAsync(
            ConfigurationId, Module.ModuleTemplateId, arch);
        if (result.IsSuccess && result.Value is not null)
            _validationStatus = result.Value;
    }

    private async Task ScheduleValidationAsync()
    {
        if (_isValidating || _selectedArch is null) return;

        _isValidating = true;
        _pollCts?.Cancel();
        _pollCts = new CancellationTokenSource();

        var result = await ModulesApiClient.ScheduleModuleValidationAsync(
            ConfigurationId, Module.ModuleTemplateId, _selectedArch.Value);

        if (result.IsFailure)
        {
            Snackbar.Add($"Failed to schedule validation: {result.Error?.Description}", Severity.Error);
            _isValidating = false;
            return;
        }

        _validationStatus = new ModuleValidationStatusResponse("Queued", null, null, null);
        StateHasChanged();

        _ = PollStatusAsync(_pollCts.Token);
    }

    private async Task PollStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && _selectedArch.HasValue)
            {
                await Task.Delay(3000, cancellationToken);

                var result = await ModulesApiClient.GetModuleValidationStatusAsync(
                    ConfigurationId, Module.ModuleTemplateId, _selectedArch.Value, cancellationToken);

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
            { x => x.TestResults, _validationStatus.Results }
        };

        var options = new DialogOptions { MaxWidth = MaxWidth.Medium, FullWidth = true, CloseOnEscapeKey = true };
        await DialogService.ShowAsync<ValidationErrorDialog>("Validation Failed", parameters, options);
    }

    private async Task OpenEditDialogAsync()
    {
        var parameters = new DialogParameters<EditModuleValuesDialog>
        {
            { x => x.ConfigurationId, ConfigurationId },
            { x => x.SystemId, SystemId },
            { x => x.Module, Module }
        };
        var dialog = await DialogService.ShowAsync<EditModuleValuesDialog>(
            $"Edit: {Module.TemplateName}", parameters);
        var result = await dialog.Result;
        if (result is { Canceled: false })
            await OnModuleUpdated.InvokeAsync();
    }

    private static string ResolveEntryValue(ConfiguredModuleEntryResponse entry)
    {
        if (entry.ListItems is { Count: > 0 })
            return string.Join(", ", entry.ListItems);

        if (!string.IsNullOrWhiteSpace(entry.Value))
            return entry.Value;

        if (entry.IntegerLowerValue.HasValue || entry.IntegerUpperValue.HasValue)
            return $"{entry.IntegerLowerValue?.ToString() ?? "-"} - {entry.IntegerUpperValue?.ToString() ?? "-"}";

        if (entry.DecimalLowerValue.HasValue || entry.DecimalUpperValue.HasValue)
            return $"{entry.DecimalLowerValue?.ToString() ?? "-"} - {entry.DecimalUpperValue?.ToString() ?? "-"}";

        return "-";
    }
}

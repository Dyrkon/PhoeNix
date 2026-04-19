using Humanizer;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Auth;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Machines;
using PhoeNix.Common.Models;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Components.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.Extensions;
using PhoeNix.WebAPP.Helpers;
using PhoeNix.WebAPP.States;
using Architecture = PhoeNix.Domain.Enums.Architecture;
using DomainMachineState = PhoeNix.Domain.Enums.MachineState;
using MachinesState = PhoeNix.WebAPP.States.MachineState;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class MachinesTable : ComponentBase, IDisposable
{
    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private IDeploymentApiClient DeploymentApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [CascadingParameter] public MachinesState MachineState { get; set; } = null!;

    [Parameter] public List<ConfigurationListResponse> Configurations { get; set; } = [];

    private MudDataGrid<MachineTableRow>? _dataGrid;

    private string? _search;
    private bool? _enabled;
    private Architecture? _architecture;
    private DomainMachineState? _machineState;
    private HashSet<MachineTableRow> _selectedItems = [];

    private IEnumerable<MachineTableRow> SelectedUpdatableItems =>
        _selectedItems.Where(m => m.Enabled && m.InstalledConfigurationId is not null);

    protected override void OnInitialized()
    {
        MachineState.StateChanged += OnMachineStateChanged;
    }

    public void Dispose()
    {
        MachineState.StateChanged -= OnMachineStateChanged;
    }

    private void OnMachineStateChanged() => InvokeAsync(StateHasChanged);

    private async Task<GridData<MachineTableRow>> LoadServerDataAsync(GridState<MachineTableRow> state,
        CancellationToken cancellationToken)
    {
        var request = new ListMachinesRequest(
            MapSortField(state),
            state.Page + 1,
            state.PageSize,
            _search,
            _enabled,
            _architecture,
            _machineState,
            MapSortDirection(state));

        var response = await MachinesApiClient.GetMachinesAsync(request);

        if (response.IsFailure || response.Value is null)
        {
            Snackbar.Add("Failed to load machines.", Severity.Error);

            return new GridData<MachineTableRow>
            {
                Items = [],
                TotalItems = 0
            };
        }

        var configurationTitles = Configurations.ToDictionary(
            configuration => configuration.Id,
            configuration => configuration.Title);

        var items = response.Value.Items
            .Select(machine => new MachineTableRow(
                machine.Id,
                machine.Title,
                machine.Enabled,
                machine.InstalledConfigurationId,
                machine.MacAddress.ToMacFormat(),
                machine.Architecture.ToArchitectureString(),
                machine.MachineState.Humanize(),
                machine.MachineState,
                ResolveInstalledConfigurationTitle(machine.InstalledConfigurationId, configurationTitles)))
            .ToList();

        return new GridData<MachineTableRow>
        {
            Items = items,
            TotalItems = response.Value.TotalItems
        };
    }

    private Task OnRowClickAsync(DataGridRowClickEventArgs<MachineTableRow> args)
    {
        NavigationManager.NavigateToMachineDetail(args.Item.Id);
        return Task.CompletedTask;
    }

    private async Task OpenCreateDialogAsync()
    {
        var dialog = await DialogService.ShowAsync<CreateMachineDialog>(
            "Create machine",
            new DialogOptions
            {
                CloseOnEscapeKey = true,
                MaxWidth = MaxWidth.Small,
                FullWidth = true
            });

        var result = await dialog.Result;

        if (result is null || result.Canceled)
            return;

        if (_dataGrid is not null)
            await _dataGrid.ReloadServerData();
    }

    private async Task UpdateSelectedAsync()
    {
        var targets = SelectedUpdatableItems.ToList();
        await Task.WhenAll(targets.Select(m => TriggerUpdateAsync(m.Id)));
        await ReloadGridAsync();
    }

    private async Task TriggerUpdateAsync(Guid machineId)
    {
        MachineState.SetUpdate(machineId, new MachineUpdateEntry(UpdateStatus.InProgress));

        var detailResult = await MachinesApiClient.GetMachineAsync(machineId);

        if (detailResult.IsFailure || detailResult.Value?.DeploymentSnapshot is null)
        {
            MachineState.SetUpdate(machineId, new MachineUpdateEntry(
                UpdateStatus.Failed,
                new ApiError("NoSnapshot", "Machine has no deployment snapshot.")));
            return;
        }

        var snapshot = detailResult.Value.DeploymentSnapshot;

        var updateResult = await DeploymentApiClient.UpdateMachineAsync(
            snapshot.ConfigurationId,
            snapshot.SystemId,
            machineId);

        MachineState.SetUpdate(machineId, updateResult.IsSuccess
            ? new MachineUpdateEntry(UpdateStatus.Success,
                ConfigurationTitle: snapshot.ConfigurationTitle,
                SystemName: snapshot.SystemName)
            : new MachineUpdateEntry(UpdateStatus.Failed,
                Error: updateResult.Error,
                ConfigurationTitle: snapshot.ConfigurationTitle,
                SystemName: snapshot.SystemName));
    }

    private async Task OpenUpdateResultDialogAsync(Guid machineId)
    {
        var entry = MachineState.GetUpdate(machineId);
        if (entry is null)
            return;

        var parameters = new DialogParameters<UpdateConfigurationResultDialog>
        {
            { x => x.IsSuccess, entry.Status == UpdateStatus.Success },
            { x => x.ErrorCode, entry.Error?.Code },
            { x => x.ErrorMessage, entry.Error?.Description },
            { x => x.ConfigurationTitle, entry.ConfigurationTitle },
            { x => x.SystemName, entry.SystemName }
        };

        await DialogService.ShowAsync<UpdateConfigurationResultDialog>("Update Result", parameters);
    }

    private async Task OnSearchChangedAsync(string? value)
    {
        _search = value;
        await ReloadGridAsync();
    }

    private async Task OnEnabledChangedAsync(bool? value)
    {
        _enabled = value;
        await ReloadGridAsync();
    }

    private async Task OnArchitectureChangedAsync(Architecture? value)
    {
        _architecture = value;
        await ReloadGridAsync();
    }

    private async Task OnMachineStateChangedAsync(DomainMachineState? value)
    {
        _machineState = value;
        await ReloadGridAsync();
    }

    private async Task ReloadGridAsync()
    {
        if (_dataGrid is not null)
            await _dataGrid.ReloadServerData();
    }

    private static MachineSortField MapSortField(GridState<MachineTableRow> state)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition?.SortBy is null)
            return MachineSortField.Title;

        return sortDefinition.SortBy switch
        {
            nameof(MachineTableRow.Title) => MachineSortField.Title,
            nameof(MachineTableRow.MacAddress) => MachineSortField.MacAddress,
            nameof(MachineTableRow.Architecture) => MachineSortField.Architecture,
            nameof(MachineTableRow.MachineState) => MachineSortField.MachineState,
            nameof(MachineTableRow.Enabled) => MachineSortField.Enabled,
            _ => MachineSortField.Title
        };
    }

    private static Common.Models.SortDirection MapSortDirection(GridState<MachineTableRow> state)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition is null)
            return Common.Models.SortDirection.Ascending;

        return sortDefinition.Descending
            ? Common.Models.SortDirection.Descending
            : Common.Models.SortDirection.Ascending;
    }

    private static string ResolveInstalledConfigurationTitle(
        Guid? configurationId,
        IReadOnlyDictionary<Guid, string> configurationTitles)
    {
        if (configurationId is null)
            return "-";

        return configurationTitles.GetValueOrDefault(configurationId.Value, "Unknown");
    }

    private sealed record MachineTableRow(
        Guid Id,
        string Title,
        bool Enabled,
        Guid? InstalledConfigurationId,
        string MacAddress,
        string Architecture,
        string MachineState,
        DomainMachineState RawMachineState,
        string InstalledConfigurationTitle);
}

using Humanizer;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Application.Models.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;
using Architecture = PhoeNix.Domain.Enums.Architecture;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class MachinesTable : ComponentBase
{
    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public List<ConfigurationListResponse> Configurations { get; set; } = [];

    private MudDataGrid<MachineTableRow>? _dataGrid;

    private string? _search;
    private bool? _enabled;
    private Architecture? _architecture;
    private MachineState? _machineState;

    private async Task<GridData<MachineTableRow>> LoadServerDataAsync(GridState<MachineTableRow> state,
        CancellationToken cancellationToken)
    {
        var request = new ListMachinesRequest(
            MapSortField(state),
            _enabled,
            _architecture,
            _machineState,
            state.Page + 1,
            state.PageSize,
            _search,
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
                machine.MacAddress.ToMacFormat(),
                machine.Architecture.ToArchitectureString(),
                machine.MachineState.Humanize(),
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

    private async Task OnMachineStateChangedAsync(MachineState? value)
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

    private static PhoeNix.Common.Models.SortDirection MapSortDirection(GridState<MachineTableRow> state)
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
        string MacAddress,
        string Architecture,
        string MachineState,
        string InstalledConfigurationTitle);
}
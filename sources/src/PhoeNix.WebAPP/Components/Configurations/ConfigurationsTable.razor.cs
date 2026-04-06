using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.Common.Models;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;
using SortDirection = PhoeNix.Common.Models.SortDirection;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfigurationsTable : ComponentBase
{
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private IDialogService DialogService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private MudDataGrid<ConfigurationTableRow>? _dataGrid;
    private string? _search;

    private async Task<GridData<ConfigurationTableRow>> LoadServerDataAsync(
        GridState<ConfigurationTableRow> state,
        CancellationToken cancellationToken)
    {
        var request = new ListConfigurationsRequest(
            state.Page + 1,
            state.PageSize,
            _search,
            MapSortField(state),
            MapSortDirection(state));

        var response = await ConfigurationsApiClient.GetConfigurationsAsync(request, cancellationToken);

        if (response.IsFailure || response.Value is null)
        {
            Snackbar.Add("Failed to load configurations.", Severity.Error);

            return new GridData<ConfigurationTableRow>
            {
                Items = [],
                TotalItems = 0
            };
        }

        var items = response.Value.Items
            .Select(configuration => new ConfigurationTableRow(
                configuration.Id,
                configuration.Title,
                configuration.Description))
            .ToList();

        return new GridData<ConfigurationTableRow>
        {
            Items = items,
            TotalItems = response.Value.TotalItems
        };
    }

    private Task OnRowClickAsync(DataGridRowClickEventArgs<ConfigurationTableRow> args)
    {
        NavigationManager.NavigateToConfigurationDetail(args.Item.Id);
        return Task.CompletedTask;
    }

    private async Task OpenCreateDialogAsync()
    {
        var dialog = await DialogService.ShowAsync<CreateConfigurationDialog>(
            "Create configuration",
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

    private async Task ReloadGridAsync()
    {
        if (_dataGrid is not null)
            await _dataGrid.ReloadServerData();
    }

    private static ConfigurationSortField MapSortField(GridState<ConfigurationTableRow> state)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition?.SortBy is null)
            return ConfigurationSortField.Title;

        return sortDefinition.SortBy switch
        {
            nameof(ConfigurationTableRow.Title) => ConfigurationSortField.Title,
            nameof(ConfigurationTableRow.Description) => ConfigurationSortField.Description,
            _ => ConfigurationSortField.Title
        };
    }

    private static SortDirection MapSortDirection(GridState<ConfigurationTableRow> state)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition is null)
            return SortDirection.Ascending;

        return sortDefinition.Descending
            ? SortDirection.Descending
            : SortDirection.Ascending;
    }

    private sealed record ConfigurationTableRow(
        Guid Id,
        string Title,
        string Description);
}
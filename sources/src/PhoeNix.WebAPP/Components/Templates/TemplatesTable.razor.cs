using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.ApiClient.Contracts;
using PhoeNix.WebAPP.Extensions;

namespace PhoeNix.WebAPP.Components.Templates;

public partial class TemplatesTable : ComponentBase
{
    [Inject] private IModulesApiClient ModulesApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private MudDataGrid<ModuleTemplateTableRow>? _dataGrid;

    private string? _search;
    private bool? _enabled;
    private ModuleType? _moduleType;

    private async Task<GridData<ModuleTemplateTableRow>> LoadServerDataAsync(
        GridState<ModuleTemplateTableRow> state,
        CancellationToken cancellationToken)
    {
        var request = new ListModuleTemplatesRequest(
            MapSortField(state),
            state.Page + 1,
            state.PageSize,
            _search,
            _enabled,
            _moduleType,
            MapSortDirection(state));

        var response = await ModulesApiClient.GetModuleTemplatesAsync(request, cancellationToken);

        if (response.IsFailure || response.Value is null)
        {
            Snackbar.Add("Failed to load module templates.", Severity.Error);

            return new GridData<ModuleTemplateTableRow>
            {
                Items = [],
                TotalItems = 0
            };
        }

        var items = response.Value.Items
            .Select(template => new ModuleTemplateTableRow(
                template.Id,
                template.Name,
                template.Enabled,
                template.Type,
                FormatArchitectures(template.SupportedArchitectures),
                0))
            .ToList();

        return new GridData<ModuleTemplateTableRow>
        {
            Items = items,
            TotalItems = response.Value.TotalItems
        };
    }

    private Task OnRowClickAsync(DataGridRowClickEventArgs<ModuleTemplateTableRow> args)
    {
        NavigationManager.NavigateToTemplatesDetail(args.Item.Id);
        return Task.CompletedTask;
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

    private async Task OnModuleTypeChangedAsync(ModuleType? value)
    {
        _moduleType = value;
        await ReloadGridAsync();
    }

    private async Task ReloadGridAsync()
    {
        if (_dataGrid is not null)
            await _dataGrid.ReloadServerData();
    }

    private static ModuleTemplateSortField MapSortField(GridState<ModuleTemplateTableRow> state)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition?.SortBy is null)
            return ModuleTemplateSortField.Name;

        return sortDefinition.SortBy switch
        {
            nameof(ModuleTemplateTableRow.Name) => ModuleTemplateSortField.Name,
            nameof(ModuleTemplateTableRow.Type) => ModuleTemplateSortField.Type,
            nameof(ModuleTemplateTableRow.Enabled) => ModuleTemplateSortField.Enabled,
            _ => ModuleTemplateSortField.Name
        };
    }

    private static Common.Models.SortDirection MapSortDirection(GridState<ModuleTemplateTableRow> state)
    {
        var sortDefinition = state.SortDefinitions.FirstOrDefault();

        if (sortDefinition is null)
            return Common.Models.SortDirection.Ascending;

        return sortDefinition.Descending
            ? Common.Models.SortDirection.Descending
            : Common.Models.SortDirection.Ascending;
    }

    private static string FormatArchitectures(IReadOnlyList<Architecture> architectures)
    {
        if (architectures.Count == 0)
            return "-";

        return string.Join(", ", architectures.Select(a => a.ToArchitectureString()));
    }

    private sealed record ModuleTemplateTableRow(
        Guid Id,
        string Name,
        bool Enabled,
        ModuleType Type,
        string ArchitecturesDisplay,
        int EntryDefinitionsCount);
}

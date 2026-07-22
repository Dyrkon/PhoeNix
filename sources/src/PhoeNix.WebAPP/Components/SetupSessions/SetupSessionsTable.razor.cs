using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Setup;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;

namespace PhoeNix.WebAPP.Components.SetupSessions;

public partial class SetupSessionsTable : ComponentBase
{
    [Inject] private ISetupApiClient SetupApiClient { get; set; } = null!;
    [Inject] private NavigationManager NavigationManager { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private MudDataGrid<SetupSessionTableRow>? _dataGrid;

    private async Task<GridData<SetupSessionTableRow>> LoadServerDataAsync(
        GridState<SetupSessionTableRow> state,
        CancellationToken cancellationToken)
    {
        var response = await SetupApiClient.GetSessionsAsync(
            state.Page + 1,
            state.PageSize,
            cancellationToken);

        if (response.IsFailure || response.Value is null)
        {
            Snackbar.Add("Failed to load setup sessions.", Severity.Error);
            return new GridData<SetupSessionTableRow> { Items = [], TotalItems = 0 };
        }

        var items = response.Value.Items
            .Select(s => new SetupSessionTableRow(
                s.SessionId,
                s.StartTime.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"),
                s.TargetsTotal,
                s.TargetsDone,
                s.TargetsFailed))
            .ToList();

        return new GridData<SetupSessionTableRow>
        {
            Items = items,
            TotalItems = response.Value.TotalItems
        };
    }

    private Task OnRowClickAsync(DataGridRowClickEventArgs<SetupSessionTableRow> args)
    {
        NavigationManager.NavigateTo(AppRoutes.SetupSessionDetail(args.Item.SessionId));
        return Task.CompletedTask;
    }

    internal sealed record SetupSessionTableRow(
        Guid SessionId,
        string StartTime,
        int TargetsTotal,
        int TargetsDone,
        int TargetsFailed);
}
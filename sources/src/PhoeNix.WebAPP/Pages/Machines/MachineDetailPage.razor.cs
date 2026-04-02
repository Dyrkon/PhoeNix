using Humanizer;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Machines;
using PhoeNix.WebAPP.ApiClient.Abstractions;
using PhoeNix.WebAPP.Extensions;

namespace PhoeNix.WebAPP.Pages.Machines;

public partial class MachineDetailPage : ComponentBase
{
    [Inject] private IMachinesApiClient MachinesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    [Parameter] public Guid MachineId { get; set; }

    private MachineDetailResponse? _machine;
    private bool _isLoading = true;

    protected override async Task OnParametersSetAsync()
    {
        _isLoading = true;

        var response = await MachinesApiClient.GetMachineAsync(MachineId);

        if (response.IsFailure || response.Value is null)
        {
            Snackbar.Add("Failed to load machine detail.", Severity.Error);
            _machine = null;
            _isLoading = false;
            return;
        }

        _machine = response.Value;
        _isLoading = false;
    }

    private static string FormatString(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "-" : value;
    }

    private static string FormatInt(int? value)
    {
        return value?.ToString() ?? "-";
    }

    private static string FormatBool(bool? value)
    {
        if (!value.HasValue)
            return "-";

        return value.Value ? "Yes" : "No";
    }

    private static string FormatDateTime(DateTime? value)
    {
        return value?.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
    }

    private static string FormatBytes(long? value)
    {
        if (!value.HasValue)
            return "-";

        const double kilo = 1024d;
        const double mega = kilo * 1024d;
        const double giga = mega * 1024d;
        const double tera = giga * 1024d;

        var bytes = value.Value;

        if (bytes >= tera)
            return $"{bytes / tera:0.##} TB";

        if (bytes >= giga)
            return $"{bytes / giga:0.##} GB";

        if (bytes >= mega)
            return $"{bytes / mega:0.##} MB";

        if (bytes >= kilo)
            return $"{bytes / kilo:0.##} KB";

        return $"{bytes} B";
    }

    private static string FormatMacAddress(string value)
    {
        return value.ToMacFormat();
    }
}
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Application.Models.Configurations;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ConfiguredModuleCard : ComponentBase
{
    [Parameter] [EditorRequired] public ConfiguredModuleResponse Module { get; set; } = null!;

    private static string ResolveEntryValue(ConfiguredModuleEntryResponse entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.Value))
            return entry.Value;

        if (entry.IntegerLowerValue.HasValue || entry.IntegerUpperValue.HasValue)
            return $"{entry.IntegerLowerValue?.ToString() ?? "-"} - {entry.IntegerUpperValue?.ToString() ?? "-"}";

        if (entry.DecimalLowerValue.HasValue || entry.DecimalUpperValue.HasValue)
            return $"{entry.DecimalLowerValue?.ToString() ?? "-"} - {entry.DecimalUpperValue?.ToString() ?? "-"}";

        return "-";
    }
}
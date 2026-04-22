using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using PhoeNix.Contracts.Configurations;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class ImportConfigurationDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private List<ConfigurationResponse> _parsed = [];
    private string? _parseError;
    private bool _isSubmitting;

    private async Task OnFileSelectedAsync(InputFileChangeEventArgs e)
    {
        _parsed = [];
        _parseError = null;

        var file = e.File;

        if (!file.Name.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            _parseError = "Only .json files are supported.";
            return;
        }

        try
        {
            await using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var bytes = ms.ToArray();

            List<ConfigurationResponse>? items = null;

            try { items = JsonSerializer.Deserialize<List<ConfigurationResponse>>(bytes, JsonOptions); }
            catch { }

            if (items is null or { Count: 0 })
            {
                try
                {
                    var single = JsonSerializer.Deserialize<ConfigurationResponse>(bytes, JsonOptions);
                    if (single is not null && !string.IsNullOrWhiteSpace(single.Title))
                        items = [single];
                }
                catch { }
            }

            if (items is null or { Count: 0 } || items.Any(c => string.IsNullOrWhiteSpace(c.Title)))
            {
                _parseError = "Invalid file. Ensure it contains a valid configuration export or array of exports.";
                return;
            }

            _parsed = items;
        }
        catch
        {
            _parseError = "Failed to read the file.";
        }
    }

    private async Task SubmitAsync()
    {
        if (_parsed.Count == 0)
            return;

        _isSubmitting = true;

        try
        {
            int succeeded = 0, failed = 0;

            foreach (var item in _parsed)
            {
                var result = await ConfigurationsApiClient.ImportConfigurationAsync(item);
                if (result.IsSuccess) succeeded++; else failed++;
            }

            if (failed == 0)
                Snackbar.Add($"{succeeded} configuration{(succeeded != 1 ? "s" : "")} imported.", Severity.Success);
            else
                Snackbar.Add($"{succeeded} imported, {failed} failed.",
                    succeeded > 0 ? Severity.Warning : Severity.Error);

            MudDialog.Close(DialogResult.Ok(true));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();
}

using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using PhoeNix.Contracts.Modules;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Templates;

public partial class ImportModuleTemplateDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Inject] private IModulesApiClient ModulesApiClient { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private List<ModuleTemplateResponse> _parsed = [];
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

            List<ModuleTemplateResponse>? items = null;

            try { items = JsonSerializer.Deserialize<List<ModuleTemplateResponse>>(bytes, JsonOptions); }
            catch { }

            if (items is null or { Count: 0 })
            {
                try
                {
                    var single = JsonSerializer.Deserialize<ModuleTemplateResponse>(bytes, JsonOptions);
                    if (single is not null && !string.IsNullOrWhiteSpace(single.Name))
                        items = [single];
                }
                catch { }
            }

            if (items is null or { Count: 0 } || items.Any(t => string.IsNullOrWhiteSpace(t.Name)))
            {
                _parseError = "Invalid file. Ensure it contains a valid module template export or array of exports.";
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
                var result = await ModulesApiClient.ImportModuleTemplateAsync(item);
                if (result.IsSuccess) succeeded++; else failed++;
            }

            if (failed == 0)
                Snackbar.Add($"{succeeded} module template{(succeeded != 1 ? "s" : "")} imported.", Severity.Success);
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

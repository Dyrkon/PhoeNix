using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Configurations;

namespace PhoeNix.WebAPP.Components.Machines;

public partial class UpdateReviewDialog : ComponentBase
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public ConfigurationWithRevisionsResponse Configuration { get; set; }

    private Dictionary<string, string> _currentConfiguration = new();
    private Dictionary<string, string> _previousConfiguration = new();

    private void Close()
    {
        MudDialog.Close();
    }

    private void Submit()
    {
        MudDialog.Close();
    }

    protected override void OnInitialized()
    {
        var jsonOptions = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = true
        };

        _currentConfiguration = new Dictionary<string, string>
        {
            ["Inputs"] = JsonSerializer.Serialize(Configuration.Inputs, jsonOptions),
            ["Modules"] = JsonSerializer.Serialize(Configuration.Modules, jsonOptions),
            ["SystemSpecifications"] = JsonSerializer.Serialize(Configuration.Systems, jsonOptions)
        };

        var snapshotJson = Configuration.Revisions.FirstOrDefault()?.JsonSnapshot;

        if (!string.IsNullOrEmpty(snapshotJson))
        {
            var snapshotElements =
                JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(snapshotJson, jsonOptions) ??
                new Dictionary<string, JsonElement>();

            if (snapshotElements.TryGetValue("Inputs", out var inputs))
                _previousConfiguration["Inputs"] = JsonSerializer.Serialize(inputs, jsonOptions);

            if (snapshotElements.TryGetValue("Modules", out var modules))
                _previousConfiguration["Modules"] = JsonSerializer.Serialize(modules, jsonOptions);

            if (snapshotElements.TryGetValue("SystemSpecifications", out var systems))
                _previousConfiguration["SystemSpecifications"] = JsonSerializer.Serialize(systems, jsonOptions);
            else if (snapshotElements.TryGetValue("Systems", out var systemsFallback))
                _previousConfiguration["SystemSpecifications"] = JsonSerializer.Serialize(systemsFallback, jsonOptions);
        }

        DeduplicateConfigurations();
    }

    private void DeduplicateConfigurations()
    {
        var keysToCheck = _currentConfiguration.Keys.ToList();

        foreach (var key in keysToCheck)
        {
            _previousConfiguration.TryGetValue(key, out var previousJson);
            var currentJson = _currentConfiguration[key];

            if (currentJson == previousJson)
            {
                _currentConfiguration.Remove(key);
                _previousConfiguration.Remove(key);
            }
        }
    }
}
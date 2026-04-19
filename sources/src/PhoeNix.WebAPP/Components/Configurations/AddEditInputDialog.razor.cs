using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Configurations;
using PhoeNix.Contracts.Inputs;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Components.Configurations;

public partial class AddEditInputDialog : ComponentBase
{
    [Inject] private IConfigurationsApiClient ConfigurationsApiClient { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public Guid ConfigurationId { get; set; }
    [Parameter] public InputResponse? ExistingInput { get; set; }

    private string _name = string.Empty;
    private string _source = string.Empty;
    private readonly List<FollowEditorModel> _follows = [];
    private string? _errorMessage;
    private bool _isSubmitting;

    protected override void OnParametersSet()
    {
        if (ExistingInput is null)
            return;

        _name = ExistingInput.Name;
        _source = ExistingInput.Source;
        _follows.Clear();
        foreach (var follow in ExistingInput.Followers)
            _follows.Add(new FollowEditorModel { FollowName = follow.FollowName, FollowValue = follow.FollowValue });
    }

    private void AddFollow()
    {
        _follows.Add(new FollowEditorModel());
    }

    private async Task SubmitAsync()
    {
        _errorMessage = null;

        if (string.IsNullOrWhiteSpace(_name))
        {
            _errorMessage = "Input name is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(_source))
        {
            _errorMessage = "Source URL is required.";
            return;
        }

        _isSubmitting = true;

        try
        {
            var follows = _follows
                .Where(f => !string.IsNullOrWhiteSpace(f.FollowName))
                .Select(f => new InputFollowUpsertModel(f.FollowName, f.FollowValue))
                .ToList();

            if (ExistingInput is null)
            {
                var request = new CreateConfigurationInputRequest(_source.Trim(), _name.Trim(), follows);
                var result = await ConfigurationsApiClient.AddConfigurationInputAsync(ConfigurationId, request);

                if (result.IsFailure)
                {
                    _errorMessage = result.Error?.Description ?? "Failed to add input.";
                    return;
                }
            }
            else
            {
                var request = new UpdateConfigurationInputRequest(_source.Trim(), _name.Trim(), follows);
                var result = await ConfigurationsApiClient.UpdateConfigurationInputAsync(ConfigurationId, ExistingInput.Id, request);

                if (result.IsFailure)
                {
                    _errorMessage = result.Error?.Description ?? "Failed to update input.";
                    return;
                }
            }

            MudDialog.Close(DialogResult.Ok(true));
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();

    private sealed class FollowEditorModel
    {
        public string? FollowName { get; set; }
        public string? FollowValue { get; set; }
    }
}

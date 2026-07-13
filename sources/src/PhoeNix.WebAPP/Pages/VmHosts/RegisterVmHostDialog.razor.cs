using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Pages.VmHosts;

public partial class RegisterVmHostDialog : ComponentBase
{
    [Inject] private IVmHostsApiClient VmHostsApiClient { get; set; } = null!;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    private string _name = string.Empty;
    private VmHostProvider _provider = VmHostProvider.Libvirt;
    private string _host = string.Empty;
    private int? _port;
    private string? _username;
    private string? _secret;
    private string? _extraConfig;
    private bool _showSecret;
    private bool _isSaving;
    private string? _errorMessage;

    private void Cancel() => MudDialog.Cancel();

    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(_name) || string.IsNullOrWhiteSpace(_host))
        {
            _errorMessage = "Name and host are required.";
            return;
        }

        _isSaving = true;
        _errorMessage = null;

        var request = new RegisterVmHostRequest(
            _name, _provider, _host, _port, _username, _secret, _extraConfig);

        var result = await VmHostsApiClient.RegisterVmHostAsync(request);

        _isSaving = false;

        if (result.IsFailure)
        {
            _errorMessage = result.Error?.Description ?? "Registration failed.";
            return;
        }

        MudDialog.Close(DialogResult.Ok(true));
    }
}

using Microsoft.AspNetCore.Components;
using MudBlazor;
using PhoeNix.Contracts.Settings;
using PhoeNix.Domain.Enums;
using PhoeNix.WebAPP.ApiClient.Abstractions;

namespace PhoeNix.WebAPP.Pages;

public partial class Settings : ComponentBase
{
    [Inject] private ISettingsApiClient SettingsApiClient { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private SettingsFormModel _model = new();
    private bool _isLoading = true;
    private bool _isSaving;
    private string? _loadError;
    private string? _saveError;

    protected override async Task OnInitializedAsync()
    {
        var result = await SettingsApiClient.GetSettingsAsync();

        if (result.IsFailure)
            _loadError = result.Error?.Description ?? "Failed to load settings.";
        else
            _model = SettingsFormModel.FromResponse(result.Value!);

        _isLoading = false;
    }

    private async Task SaveAsync()
    {
        _isSaving = true;
        _saveError = null;

        var request = new UpdateAppSettingsRequest(
            _model.FileStorageRootPath,
            _model.SshCaKeyName,
            _model.SshCaPrincipal,
            _model.SshCaCertificateTtlHours,
            _model.SshCaKeyType,
            _model.DeployCaKeyType,
            _model.DeployCaKeyName,
            _model.DeployCaPrincipal,
            _model.DeployCaDeployUser,
            _model.DeployCaCertificateTtlDays,
            _model.HardwareProbeSshExecutable,
            _model.HardwareProbeBootstrapUser,
            _model.HardwareProbeProbeCommand,
            _model.HardwareProbeConnectTimeoutSeconds,
            _model.HardwareProbeProbeTimeoutSeconds,
            _model.HardwareProbeDisableHostKeyChecking,
            _model.InstallerExecutableName,
            _model.InstallerTargetUser,
            _model.InstallerTimeoutMinutes,
            _model.InstallerDisableHostKeyChecking,
            _model.InstallerBuildOnTarget,
            _model.InstallerCopyHostKeys,
            _model.UpdaterBuildHost,
            _model.UpdaterUseRemoteSudo,
            _model.UpdaterFast,
            _model.MonitoringPrometheusEndpoint,
            _model.MonitoringTokenTtlDays,
            _model.MonitoringAddressResolution,
            _model.NetbootApiBasePublicUrl,
            _model.NetbootHostExecutablePath,
            _model.NetbootListenAddress,
            _model.NetbootPort);

        var result = await SettingsApiClient.UpdateSettingsAsync(request);

        if (result.IsFailure)
            _saveError = result.Error?.Description ?? "Failed to save settings.";
        else
            Snackbar.Add("Settings saved successfully.", Severity.Success);

        _isSaving = false;
    }

    private sealed class SettingsFormModel
    {
        public string FileStorageRootPath { get; set; } = "/var/lib/phoenix";

        public string SshCaKeyName { get; set; } = "phoenix_user_ca";
        public string SshCaPrincipal { get; set; } = "root";
        public double SshCaCertificateTtlHours { get; set; } = 1;
        public string SshCaKeyType { get; set; } = "ed25519";

        public string DeployCaKeyType { get; set; } = "ed25519";
        public string DeployCaKeyName { get; set; } = "phoenix-deploy-user-ca";
        public string DeployCaPrincipal { get; set; } = "phoenix-deploy";
        public string DeployCaDeployUser { get; set; } = "phoenix-deploy";
        public double DeployCaCertificateTtlDays { get; set; } = 365;

        public string HardwareProbeSshExecutable { get; set; } = "ssh";
        public string HardwareProbeBootstrapUser { get; set; } = "root";
        public string HardwareProbeProbeCommand { get; set; } = "nixos-facter";
        public int HardwareProbeConnectTimeoutSeconds { get; set; } = 30;
        public int HardwareProbeProbeTimeoutSeconds { get; set; } = 120;
        public bool HardwareProbeDisableHostKeyChecking { get; set; } = true;

        public string InstallerExecutableName { get; set; } = "nixos-anywhere";
        public string InstallerTargetUser { get; set; } = "root";
        public int InstallerTimeoutMinutes { get; set; } = 90;
        public bool InstallerDisableHostKeyChecking { get; set; } = true;
        public bool InstallerBuildOnTarget { get; set; }
        public bool InstallerCopyHostKeys { get; set; }

        public string UpdaterBuildHost { get; set; } = "";
        public bool UpdaterUseRemoteSudo { get; set; } = true;
        public bool UpdaterFast { get; set; } = true;

        public string MonitoringPrometheusEndpoint { get; set; } = "http://localhost:9090/prometheus";
        public double MonitoringTokenTtlDays { get; set; } = 7;
        public MonitoringAddressResolution MonitoringAddressResolution { get; set; } = MonitoringAddressResolution.MdnsHostname;

        public string NetbootApiBasePublicUrl { get; set; } = "http://YOUR-API-OR-HOSTNAME:8888/api";
        public string NetbootHostExecutablePath { get; set; } = "pixiecore";
        public string NetbootListenAddress { get; set; } = "0.0.0.0";
        public int NetbootPort { get; set; } = 64172;

        public static SettingsFormModel FromResponse(AppSettingsResponse r)
        {
            return new SettingsFormModel
            {
                FileStorageRootPath = r.FileStorageRootPath,
                SshCaKeyName = r.SshCaKeyName,
                SshCaPrincipal = r.SshCaPrincipal,
                SshCaCertificateTtlHours = r.SshCaCertificateTtlHours,
                SshCaKeyType = r.SshCaKeyType,
                DeployCaKeyType = r.DeployCaKeyType,
                DeployCaKeyName = r.DeployCaKeyName,
                DeployCaPrincipal = r.DeployCaPrincipal,
                DeployCaDeployUser = r.DeployCaDeployUser,
                DeployCaCertificateTtlDays = r.DeployCaCertificateTtlDays,
                HardwareProbeSshExecutable = r.HardwareProbeSshExecutable,
                HardwareProbeBootstrapUser = r.HardwareProbeBootstrapUser,
                HardwareProbeProbeCommand = r.HardwareProbeProbeCommand,
                HardwareProbeConnectTimeoutSeconds = r.HardwareProbeConnectTimeoutSeconds,
                HardwareProbeProbeTimeoutSeconds = r.HardwareProbeProbeTimeoutSeconds,
                HardwareProbeDisableHostKeyChecking = r.HardwareProbeDisableHostKeyChecking,
                InstallerExecutableName = r.InstallerExecutableName,
                InstallerTargetUser = r.InstallerTargetUser,
                InstallerTimeoutMinutes = r.InstallerTimeoutMinutes,
                InstallerDisableHostKeyChecking = r.InstallerDisableHostKeyChecking,
                InstallerBuildOnTarget = r.InstallerBuildOnTarget,
                InstallerCopyHostKeys = r.InstallerCopyHostKeys,
                UpdaterBuildHost = r.UpdaterBuildHost,
                UpdaterUseRemoteSudo = r.UpdaterUseRemoteSudo,
                UpdaterFast = r.UpdaterFast,
                MonitoringPrometheusEndpoint = r.MonitoringPrometheusEndpoint,
                MonitoringTokenTtlDays = r.MonitoringTokenTtlDays,
                MonitoringAddressResolution = r.MonitoringAddressResolution,
                NetbootApiBasePublicUrl = r.NetbootApiBasePublicUrl,
                NetbootHostExecutablePath = r.NetbootHostExecutablePath,
                NetbootListenAddress = r.NetbootListenAddress,
                NetbootPort = r.NetbootPort
            };
        }
    }
}
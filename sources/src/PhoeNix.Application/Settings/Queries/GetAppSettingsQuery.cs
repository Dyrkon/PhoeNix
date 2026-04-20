using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Settings.Queries;

public sealed record GetAppSettingsQuery : IQuery<AppSettingsResponse>;

internal sealed class GetAppSettingsQueryHandler(IAppSettingsRepository settingsRepository)
    : IQueryHandler<GetAppSettingsQuery, AppSettingsResponse>
{
    public Task<Result<AppSettingsResponse>> Handle(
        GetAppSettingsQuery request,
        CancellationToken cancellationToken)
    {
        return settingsRepository
            .GetAsync(cancellationToken)
            .EnsureNotNull(new Error("AppSettings.NotFound", "Application settings have not been initialized."))
            .Map(settings => new AppSettingsResponse(
                settings.FileStorageRootPath,
                settings.SshCaKeyName,
                settings.SshCaPrincipal,
                settings.SshCaCertificateTtlHours,
                settings.SshCaKeyType,
                settings.DeployCaKeyType,
                settings.DeployCaKeyName,
                settings.DeployCaPrincipal,
                settings.DeployCaDeployUser,
                settings.DeployCaCertificateTtlDays,
                settings.HardwareProbeSshExecutable,
                settings.HardwareProbeBootstrapUser,
                settings.HardwareProbeProbeCommand,
                settings.HardwareProbeConnectTimeoutSeconds,
                settings.HardwareProbeProbeTimeoutSeconds,
                settings.HardwareProbeDisableHostKeyChecking,
                settings.InstallerExecutableName,
                settings.InstallerTargetUser,
                settings.InstallerTimeoutMinutes,
                settings.InstallerDisableHostKeyChecking,
                settings.InstallerBuildOnTarget,
                settings.InstallerCopyHostKeys,
                settings.UpdaterBuildHost,
                settings.UpdaterUseRemoteSudo,
                settings.UpdaterFast,
                settings.MonitoringPrometheusEndpoint,
                settings.MonitoringTokenTtlDays,
                settings.NetbootApiBasePublicUrl,
                settings.NetbootHostExecutablePath,
                settings.NetbootListenAddress,
                settings.NetbootPort));
    }
}
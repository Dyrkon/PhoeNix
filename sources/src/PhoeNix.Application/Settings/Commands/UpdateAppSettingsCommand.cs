using PhoeNix.Application.Abstractions.Authentication;
using PhoeNix.Application.Abstractions.Messaging;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Extensions;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Settings.Commands;

public sealed record UpdateAppSettingsCommand(
    string FileStorageRootPath,
    string SshCaKeyName,
    string SshCaPrincipal,
    double SshCaCertificateTtlHours,
    string SshCaKeyType,
    string DeployCaKeyType,
    string DeployCaKeyName,
    string DeployCaPrincipal,
    string DeployCaDeployUser,
    double DeployCaCertificateTtlDays,
    string HardwareProbeSshExecutable,
    string HardwareProbeBootstrapUser,
    string HardwareProbeProbeCommand,
    int HardwareProbeConnectTimeoutSeconds,
    int HardwareProbeProbeTimeoutSeconds,
    bool HardwareProbeDisableHostKeyChecking,
    string InstallerExecutableName,
    string InstallerTargetUser,
    int InstallerTimeoutMinutes,
    bool InstallerDisableHostKeyChecking,
    bool InstallerBuildOnTarget,
    bool InstallerCopyHostKeys,
    string UpdaterBuildHost,
    bool UpdaterUseRemoteSudo,
    bool UpdaterFast,
    string MonitoringPrometheusEndpoint,
    double MonitoringTokenTtlDays,
    string NetbootApiBasePublicUrl,
    string NetbootHostExecutablePath,
    string NetbootListenAddress,
    int NetbootPort) : ICommand;

internal sealed class UpdateAppSettingsCommandHandler(
    IAppSettingsRepository settingsRepository,
    ICurrentUserAccessor currentUserAccessor)
    : ICommandHandler<UpdateAppSettingsCommand>
{
    public Task<Result> Handle(
        UpdateAppSettingsCommand request,
        CancellationToken cancellationToken)
    {
        var userIdResult = currentUserAccessor.GetUserId();
        if (userIdResult.IsFailure)
            return Task.FromResult(Result.Failure(userIdResult.Error));

        return settingsRepository
            .GetAsync(userIdResult.Value, cancellationToken)
            .EnsureNotNull(new Error("AppSettings.NotFound", "Application settings have not been initialized."))
            .Bind(settings =>
            {
                settings.Update(
                    request.FileStorageRootPath,
                    request.SshCaKeyName,
                    request.SshCaPrincipal,
                    request.SshCaCertificateTtlHours,
                    request.SshCaKeyType,
                    request.DeployCaKeyType,
                    request.DeployCaKeyName,
                    request.DeployCaPrincipal,
                    request.DeployCaDeployUser,
                    request.DeployCaCertificateTtlDays,
                    request.HardwareProbeSshExecutable,
                    request.HardwareProbeBootstrapUser,
                    request.HardwareProbeProbeCommand,
                    request.HardwareProbeConnectTimeoutSeconds,
                    request.HardwareProbeProbeTimeoutSeconds,
                    request.HardwareProbeDisableHostKeyChecking,
                    request.InstallerExecutableName,
                    request.InstallerTargetUser,
                    request.InstallerTimeoutMinutes,
                    request.InstallerDisableHostKeyChecking,
                    request.InstallerBuildOnTarget,
                    request.InstallerCopyHostKeys,
                    request.UpdaterBuildHost,
                    request.UpdaterUseRemoteSudo,
                    request.UpdaterFast,
                    request.MonitoringPrometheusEndpoint,
                    request.MonitoringTokenTtlDays,
                    request.NetbootApiBasePublicUrl,
                    request.NetbootHostExecutablePath,
                    request.NetbootListenAddress,
                    request.NetbootPort);

                return Result.Success();
            });
    }
}

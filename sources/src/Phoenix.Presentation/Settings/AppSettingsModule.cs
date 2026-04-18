using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using PhoeNix.Application.Models.Settings;
using PhoeNix.Application.Settings.Commands;
using PhoeNix.Application.Settings.Queries;
using Phoenix.Presentation.Extensions;

namespace Phoenix.Presentation.Settings;

public class AppSettingsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/settings", GetSettings)
            .Produces<AppSettingsResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPut("/settings", UpdateSettings)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> GetSettings(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetAppSettingsQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.AsHttpResult();
    }

    private async Task<IResult> UpdateSettings(
        UpdateAppSettingsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAppSettingsCommand(
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

        var result = await sender.Send(command, cancellationToken);
        return result.AsHttpResult();
    }
}

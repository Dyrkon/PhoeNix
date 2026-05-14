using FluentAssertions;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Users;

namespace PhoeNix.Domain.UnitTests;

public class AppSettingsTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private readonly AppSettingsId _id = new(Guid.NewGuid());

    [Fact]
    public void AppSettings_Should_CreateDefault_With_Known_Values()
    {
        var settings = AppSettings.CreateDefault(_id, OwnerId);

        settings.Id.Should().Be(_id);
        settings.FileStorageRootPath.Should().Be("/var/lib/phoenix");
        settings.SshCaKeyName.Should().Be("phoenix_user_ca");
        settings.SshCaPrincipal.Should().Be("root");
        settings.SshCaCertificateTtlHours.Should().Be(1);
        settings.SshCaKeyType.Should().Be("ed25519");
        settings.DeployCaKeyType.Should().Be("ed25519");
        settings.DeployCaKeyName.Should().Be("phoenix-deploy-user-ca");
        settings.DeployCaPrincipal.Should().Be("phoenix-deploy");
        settings.DeployCaDeployUser.Should().Be("phoenix-deploy");
        settings.DeployCaCertificateTtlDays.Should().Be(365);
        settings.HardwareProbeSshExecutable.Should().Be("ssh");
        settings.HardwareProbeBootstrapUser.Should().Be("root");
        settings.HardwareProbeProbeCommand.Should().Be("nixos-facter");
        settings.HardwareProbeConnectTimeoutSeconds.Should().Be(30);
        settings.HardwareProbeProbeTimeoutSeconds.Should().Be(120);
        settings.HardwareProbeDisableHostKeyChecking.Should().BeTrue();
        settings.InstallerExecutableName.Should().Be("nixos-anywhere");
        settings.InstallerTargetUser.Should().Be("root");
        settings.InstallerTimeoutMinutes.Should().Be(90);
        settings.InstallerDisableHostKeyChecking.Should().BeTrue();
        settings.InstallerBuildOnTarget.Should().BeFalse();
        settings.InstallerCopyHostKeys.Should().BeFalse();
        settings.UpdaterBuildHost.Should().Be("");
        settings.UpdaterUseRemoteSudo.Should().BeTrue();
        settings.UpdaterFast.Should().BeFalse();
        settings.MonitoringPrometheusEndpoint.Should().Be("http://localhost:9090/prometheus");
        settings.MonitoringTokenTtlDays.Should().Be(7);
        settings.NetbootPort.Should().Be(64172);
    }

    [Fact]
    public void AppSettings_Should_Update_All_Properties()
    {
        var settings = AppSettings.CreateDefault(_id, OwnerId);

        settings.Update(
            "/data/storage",
            "my-ssh-ca",
            "admin",
            48,
            "rsa",
            "ecdsa",
            "my-deploy-ca",
            "deploy-principal",
            "deploy-user",
            30,
            "/usr/bin/ssh",
            "bootstrap",
            "probe-cmd",
            60,
            300,
            false,
            "installer",
            "install-user",
            120,
            false,
            false,
            true,
            "build-host",
            false,
            true,
            "http://prom:9090",
            14,
            Enums.MonitoringAddressResolution.LastKnownIp,
            "lan",
            "http://api:8888",
            "/usr/bin/pixiecore",
            "127.0.0.1",
            1234);

        settings.FileStorageRootPath.Should().Be("/data/storage");
        settings.SshCaKeyName.Should().Be("my-ssh-ca");
        settings.SshCaPrincipal.Should().Be("admin");
        settings.SshCaCertificateTtlHours.Should().Be(48);
        settings.SshCaKeyType.Should().Be("rsa");
        settings.DeployCaKeyType.Should().Be("ecdsa");
        settings.DeployCaKeyName.Should().Be("my-deploy-ca");
        settings.DeployCaPrincipal.Should().Be("deploy-principal");
        settings.DeployCaDeployUser.Should().Be("deploy-user");
        settings.DeployCaCertificateTtlDays.Should().Be(30);
        settings.HardwareProbeSshExecutable.Should().Be("/usr/bin/ssh");
        settings.HardwareProbeBootstrapUser.Should().Be("bootstrap");
        settings.HardwareProbeProbeCommand.Should().Be("probe-cmd");
        settings.HardwareProbeConnectTimeoutSeconds.Should().Be(60);
        settings.HardwareProbeProbeTimeoutSeconds.Should().Be(300);
        settings.HardwareProbeDisableHostKeyChecking.Should().BeFalse();
        settings.InstallerExecutableName.Should().Be("installer");
        settings.InstallerTargetUser.Should().Be("install-user");
        settings.InstallerTimeoutMinutes.Should().Be(120);
        settings.InstallerDisableHostKeyChecking.Should().BeFalse();
        settings.InstallerBuildOnTarget.Should().BeFalse();
        settings.InstallerCopyHostKeys.Should().BeTrue();
        settings.UpdaterBuildHost.Should().Be("build-host");
        settings.UpdaterUseRemoteSudo.Should().BeFalse();
        settings.UpdaterFast.Should().BeTrue();
        settings.MonitoringPrometheusEndpoint.Should().Be("http://prom:9090");
        settings.MonitoringTokenTtlDays.Should().Be(14);
        settings.NetbootApiBasePublicUrl.Should().Be("http://api:8888");
        settings.LocalDomain.Should().Be("lan");
        settings.NetbootHostExecutablePath.Should().Be("/usr/bin/pixiecore");
        settings.NetbootListenAddress.Should().Be("127.0.0.1");
        settings.NetbootPort.Should().Be(1234);
    }
}
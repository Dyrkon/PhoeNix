using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;

// ReSharper disable PropertyCanBeMadeInitOnly.Local

namespace PhoeNix.Domain.Entities.AppSettings;

public class AppSettings : AggregateRoot<AppSettingsId>
{
    private AppSettings(AppSettingsId id) : base(id)
    {
    }

    public UserId OwnerId { get; private set; } = default!;

    public string FileStorageRootPath { get; private set; } = default!;

    public string SshCaKeyName { get; private set; } = default!;
    public string SshCaPrincipal { get; private set; } = default!;
    public double SshCaCertificateTtlHours { get; private set; }
    public string SshCaKeyType { get; private set; } = default!;

    public string DeployCaKeyType { get; private set; } = default!;
    public string DeployCaKeyName { get; private set; } = default!;
    public string DeployCaPrincipal { get; private set; } = default!;
    public string DeployCaDeployUser { get; private set; } = default!;
    public double DeployCaCertificateTtlDays { get; private set; }

    public string HardwareProbeSshExecutable { get; private set; } = default!;
    public string HardwareProbeBootstrapUser { get; private set; } = default!;
    public string HardwareProbeProbeCommand { get; private set; } = default!;
    public int HardwareProbeConnectTimeoutSeconds { get; private set; }
    public int HardwareProbeProbeTimeoutSeconds { get; private set; }
    public bool HardwareProbeDisableHostKeyChecking { get; private set; }

    public string InstallerExecutableName { get; private set; } = default!;
    public string InstallerTargetUser { get; private set; } = default!;
    public int InstallerTimeoutMinutes { get; private set; }
    public bool InstallerDisableHostKeyChecking { get; private set; }
    public bool InstallerBuildOnTarget { get; private set; }
    public bool InstallerCopyHostKeys { get; private set; }

    public string UpdaterBuildHost { get; private set; } = default!;
    public bool UpdaterUseRemoteSudo { get; private set; }
    public bool UpdaterFast { get; private set; } = false;

    public string MonitoringPrometheusEndpoint { get; private set; } = default!;
    public double MonitoringTokenTtlDays { get; private set; }
    public MonitoringAddressResolution MonitoringAddressResolution { get; private set; }
    public string LocalDomain { get; private set; }

    public string NetbootApiBasePublicUrl { get; private set; } = default!;
    public string NetbootHostExecutablePath { get; private set; } = default!;
    public string NetbootListenAddress { get; private set; } = default!;
    public int NetbootPort { get; private set; }

    public GitSyncMode GitSyncMode { get; private set; }
    public string GitRemoteUrl { get; private set; } = string.Empty;
    public string GitBranch { get; private set; } = "main";
    public GitAuthMethod GitAuthMethod { get; private set; }
    public string GitAuthSecret { get; private set; } = string.Empty;
    public bool GitPushNixFiles { get; private set; }
    public ValidationTier GitPushValidationTier { get; private set; }
    public int? GitPullPollingIntervalMinutes { get; private set; }
    public bool GitPullDeleteOrphans { get; private set; }

    public void Update(
        string fileStorageRootPath,
        string sshCaKeyName,
        string sshCaPrincipal,
        double sshCaCertificateTtlHours,
        string sshCaKeyType,
        string deployCaKeyType,
        string deployCaKeyName,
        string deployCaPrincipal,
        string deployCaDeployUser,
        double deployCaCertificateTtlDays,
        string hardwareProbeSshExecutable,
        string hardwareProbeBootstrapUser,
        string hardwareProbeProbeCommand,
        int hardwareProbeConnectTimeoutSeconds,
        int hardwareProbeProbeTimeoutSeconds,
        bool hardwareProbeDisableHostKeyChecking,
        string installerExecutableName,
        string installerTargetUser,
        int installerTimeoutMinutes,
        bool installerDisableHostKeyChecking,
        bool installerBuildOnTarget,
        bool installerCopyHostKeys,
        string updaterBuildHost,
        bool updaterUseRemoteSudo,
        bool updaterFast,
        string monitoringPrometheusEndpoint,
        double monitoringTokenTtlDays,
        MonitoringAddressResolution monitoringAddressResolution,
        string localDomain,
        string netbootApiBasePublicUrl,
        string netbootHostExecutablePath,
        string netbootListenAddress,
        int netbootPort,
        GitSyncMode gitSyncMode,
        string gitRemoteUrl,
        string gitBranch,
        GitAuthMethod gitAuthMethod,
        string gitAuthSecret,
        bool gitPushNixFiles,
        ValidationTier gitPushValidationTier,
        int? gitPullPollingIntervalMinutes,
        bool gitPullDeleteOrphans)
    {
        FileStorageRootPath = fileStorageRootPath;
        SshCaKeyName = sshCaKeyName;
        SshCaPrincipal = sshCaPrincipal;
        SshCaCertificateTtlHours = sshCaCertificateTtlHours;
        SshCaKeyType = sshCaKeyType;
        DeployCaKeyType = deployCaKeyType;
        DeployCaKeyName = deployCaKeyName;
        DeployCaPrincipal = deployCaPrincipal;
        DeployCaDeployUser = deployCaDeployUser;
        DeployCaCertificateTtlDays = deployCaCertificateTtlDays;
        HardwareProbeSshExecutable = hardwareProbeSshExecutable;
        HardwareProbeBootstrapUser = hardwareProbeBootstrapUser;
        HardwareProbeProbeCommand = hardwareProbeProbeCommand;
        HardwareProbeConnectTimeoutSeconds = hardwareProbeConnectTimeoutSeconds;
        HardwareProbeProbeTimeoutSeconds = hardwareProbeProbeTimeoutSeconds;
        HardwareProbeDisableHostKeyChecking = hardwareProbeDisableHostKeyChecking;
        InstallerExecutableName = installerExecutableName;
        InstallerTargetUser = installerTargetUser;
        InstallerTimeoutMinutes = installerTimeoutMinutes;
        InstallerDisableHostKeyChecking = installerDisableHostKeyChecking;
        InstallerBuildOnTarget = installerBuildOnTarget;
        InstallerCopyHostKeys = installerCopyHostKeys;
        UpdaterBuildHost = updaterBuildHost;
        UpdaterUseRemoteSudo = updaterUseRemoteSudo;
        UpdaterFast = updaterFast;
        MonitoringPrometheusEndpoint = monitoringPrometheusEndpoint;
        MonitoringTokenTtlDays = monitoringTokenTtlDays;
        MonitoringAddressResolution = monitoringAddressResolution;
        LocalDomain = localDomain;
        NetbootApiBasePublicUrl = netbootApiBasePublicUrl;
        NetbootHostExecutablePath = netbootHostExecutablePath;
        NetbootListenAddress = netbootListenAddress;
        NetbootPort = netbootPort;
        GitSyncMode = gitSyncMode;
        GitRemoteUrl = gitRemoteUrl;
        GitBranch = gitBranch;
        GitAuthMethod = gitAuthMethod;
        GitAuthSecret = gitAuthSecret;
        GitPushNixFiles = gitPushNixFiles;
        GitPushValidationTier = gitPushValidationTier;
        GitPullPollingIntervalMinutes = gitPullPollingIntervalMinutes;
        GitPullDeleteOrphans = gitPullDeleteOrphans;
    }

    public static AppSettings CreateDefault(AppSettingsId id, UserId ownerId)
    {
        return new AppSettings(id)
        {
            OwnerId = ownerId,
            FileStorageRootPath = "/var/lib/phoenix",
            SshCaKeyName = "phoenix_user_ca",
            SshCaPrincipal = "root",
            SshCaCertificateTtlHours = 1,
            SshCaKeyType = "ed25519",
            DeployCaKeyType = "ed25519",
            DeployCaKeyName = "phoenix-deploy-user-ca",
            DeployCaPrincipal = "phoenix-deploy",
            DeployCaDeployUser = "phoenix-deploy",
            DeployCaCertificateTtlDays = 365,
            HardwareProbeSshExecutable = "ssh",
            HardwareProbeBootstrapUser = "root",
            HardwareProbeProbeCommand = "nixos-facter",
            HardwareProbeConnectTimeoutSeconds = 30,
            HardwareProbeProbeTimeoutSeconds = 120,
            HardwareProbeDisableHostKeyChecking = true,
            InstallerExecutableName = "nixos-anywhere",
            InstallerTargetUser = "root",
            InstallerTimeoutMinutes = 90,
            InstallerDisableHostKeyChecking = true,
            InstallerBuildOnTarget = false,
            InstallerCopyHostKeys = false,
            UpdaterBuildHost = "",
            UpdaterUseRemoteSudo = true,
            UpdaterFast = false,
            MonitoringPrometheusEndpoint = "http://localhost:9090/prometheus",
            MonitoringTokenTtlDays = 7,
            MonitoringAddressResolution = MonitoringAddressResolution.MdnsHostname,
            LocalDomain = "lan",
            NetbootApiBasePublicUrl = "http://YOUR-API-OR-HOSTNAME:8888/api",
            NetbootHostExecutablePath = "/run/wrappers/bin/pixiecore",
            NetbootListenAddress = "0.0.0.0",
            NetbootPort = 64172,
            GitSyncMode = GitSyncMode.None,
            GitRemoteUrl = "",
            GitBranch = "main",
            GitAuthMethod = GitAuthMethod.None,
            GitAuthSecret = "",
            GitPushNixFiles = false,
            GitPushValidationTier = ValidationTier.None,
            GitPullPollingIntervalMinutes = null,
            GitPullDeleteOrphans = false
        };
    }
}
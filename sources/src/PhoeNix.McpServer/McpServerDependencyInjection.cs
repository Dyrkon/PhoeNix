using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Options;
using PhoeNix.Persistence;

namespace PhoeNix.McpServer;

public static class McpServerDependencyInjection
{
    public static IServiceCollection AddMcpServerOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<FileStorageOptions>()
            .BindConfiguration("FileStorage");

        services.AddOptions<SshKeyStorageOptions>()
            .BindConfiguration("SshKeyFileStore");

        services.AddOptions<SshCaOptions>()
            .BindConfiguration("SshCa");

        services.AddOptions<JwtCallbackTokenOptions>()
            .BindConfiguration("CallbackToken");

        services.AddOptions<NetbootHostOptions>()
            .BindConfiguration("NetbootHost");

        services.AddOptions<HardwareProbeOptions>()
            .BindConfiguration("HardwareProbe");

        services.AddOptions<NixosInstallerOptions>()
            .BindConfiguration("NixosInstaller");

        services.AddOptions<NixOsUpdaterOptions>()
            .BindConfiguration("NixosUpdater");

        services.AddOptions<MonitoringOptions>()
            .BindConfiguration("Monitoring");

        return services;
    }

    public static IServiceCollection AddMcpHost(this IServiceCollection services)
    {
        services.AddSeeding();
        return services;
    }
}
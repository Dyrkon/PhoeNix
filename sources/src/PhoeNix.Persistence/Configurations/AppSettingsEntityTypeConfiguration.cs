using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.AppSettings;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class AppSettingsEntityTypeConfiguration : IApplicationEntityTypeConfiguration<AppSettings>
{
    public void Configure(EntityTypeBuilder<AppSettings> builder)
    {
        builder.HasKey(s => s.Id);

        builder
            .Property(s => s.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => new AppSettingsId(value));

        builder.Property(s => s.OwnerId)
            .IsRequired()
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.HasIndex(s => s.OwnerId)
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.FileStorageRootPath).IsRequired().HasMaxLength(500);

        builder.Property(s => s.SshCaKeyName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.SshCaPrincipal).IsRequired().HasMaxLength(200);
        builder.Property(s => s.SshCaCertificateTtlHours).IsRequired();
        builder.Property(s => s.SshCaKeyType).IsRequired().HasMaxLength(50);

        builder.Property(s => s.DeployCaKeyType).IsRequired().HasMaxLength(50);
        builder.Property(s => s.DeployCaKeyName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.DeployCaPrincipal).IsRequired().HasMaxLength(200);
        builder.Property(s => s.DeployCaDeployUser).IsRequired().HasMaxLength(200);
        builder.Property(s => s.DeployCaCertificateTtlDays).IsRequired();

        builder.Property(s => s.HardwareProbeSshExecutable).IsRequired().HasMaxLength(200);
        builder.Property(s => s.HardwareProbeBootstrapUser).IsRequired().HasMaxLength(200);
        builder.Property(s => s.HardwareProbeProbeCommand).IsRequired().HasMaxLength(200);
        builder.Property(s => s.HardwareProbeConnectTimeoutSeconds).IsRequired();
        builder.Property(s => s.HardwareProbeProbeTimeoutSeconds).IsRequired();
        builder.Property(s => s.HardwareProbeDisableHostKeyChecking).IsRequired();

        builder.Property(s => s.InstallerExecutableName).IsRequired().HasMaxLength(200);
        builder.Property(s => s.InstallerTargetUser).IsRequired().HasMaxLength(200);
        builder.Property(s => s.InstallerTimeoutMinutes).IsRequired();
        builder.Property(s => s.InstallerDisableHostKeyChecking).IsRequired();
        builder.Property(s => s.InstallerBuildOnTarget).IsRequired();
        builder.Property(s => s.InstallerCopyHostKeys).IsRequired();

        builder.Property(s => s.UpdaterBuildHost).IsRequired().HasMaxLength(500);
        builder.Property(s => s.UpdaterUseRemoteSudo).IsRequired();
        builder.Property(s => s.UpdaterFast).IsRequired();

        builder.Property(s => s.MonitoringPrometheusEndpoint).IsRequired().HasMaxLength(500);
        builder.Property(s => s.MonitoringTokenTtlDays).IsRequired();
        builder.Property(s => s.MonitoringAddressResolution).IsRequired();

        builder.Property(s => s.NetbootApiBasePublicUrl).IsRequired().HasMaxLength(500);
        builder.Property(s => s.NetbootHostExecutablePath).IsRequired().HasMaxLength(500);
        builder.Property(s => s.NetbootListenAddress).IsRequired().HasMaxLength(200);
        builder.Property(s => s.NetbootPort).IsRequired();
    }
}

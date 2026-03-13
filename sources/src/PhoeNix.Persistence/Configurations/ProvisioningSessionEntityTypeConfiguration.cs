using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.ProvisioningSessions;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class ProvisioningSessionEntityTypeConfiguration
    : IApplicationEntityTypeConfiguration<ProvisioningSession>
{
    public void Configure(EntityTypeBuilder<ProvisioningSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new ProvisioningSessionId(value))
            .ValueGeneratedNever();

        builder.OwnsOne(x => x.BootArtefactDescriptor, owned =>
        {
            owned.Property(p => p.Kernel).HasColumnName("KernelLocation");
            owned.Property(p => p.RamDisk).HasColumnName("InitRdLocation");
            owned.Property(p => p.Init).HasColumnName("CmdLine");
        });

        builder.OwnsOne(x => x.SshCredential, owned =>
        {
            owned.Property(p => p.PublicKey).HasColumnName("SshPublicKey");
            owned.Property(p => p.ExpiresAtUtc).HasColumnName("SshKeyExpiresAtUtc");
            owned.Property(p => p.RevokedAtUtc).HasColumnName("SshKeyRevokedAtUtc");
            owned.Property(p => p.CertificatePublicKey).HasColumnName("SshCertificatePublicKey");
        });

        var targets = builder.OwnsMany(x => x.Targets, owned =>
        {
            owned.ToTable("ProvisioningSessionTargets");

            owned.WithOwner().HasForeignKey("ProvisioningSessionId");

            owned.Property(t => t.MachineId)
                .HasColumnName("MachineId")
                .HasConversion(
                    id => id.Value,
                    value => new MachineId(value))
                .IsRequired();

            owned.Property(t => t.Stage)
                .HasConversion<string>()
                .IsRequired();

            owned.OwnsOne(t => t.CallbackToken, token =>
            {
                token.Property(p => p.Token).HasColumnName("CallbackToken");
                token.Property(p => p.ExpiresAtUtc).HasColumnName("CallbackTokenExpiresAtUtc");
                token.Property(p => p.RevokedAtUtc).HasColumnName("CallbackTokenRevokedAtUtc");
            });

            owned.HasKey("ProvisioningSessionId", "MachineId");
            owned.HasIndex("MachineId");
        });

        builder.Navigation(x => x.BootArtefactDescriptor).IsRequired(false);
        builder.Navigation(x => x.SshCredential).IsRequired(false);
    }
}
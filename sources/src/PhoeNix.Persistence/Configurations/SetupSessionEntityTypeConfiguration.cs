using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class SetupSessionEntityTypeConfiguration
    : IApplicationEntityTypeConfiguration<SetupSession>
{
    public void Configure(EntityTypeBuilder<SetupSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(
                id => id.Value,
                value => new SetupSessionId(value))
            .ValueGeneratedNever();

        builder.Property(x => x.StartTime)
            .IsRequired();

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

        builder.OwnsMany(x => x.Targets, owned =>
        {
            owned.ToTable("SetupSessionTargets");

            owned.WithOwner()
                .HasForeignKey("SetupSessionId");

            owned.Property(t => t.MachineId)
                .HasColumnName("MachineId")
                .HasConversion(
                    id => id.Value,
                    value => new MachineId(value))
                .IsRequired();

            owned.Property(t => t.Stage)
                .HasConversion<string>()
                .IsRequired();

            owned.Property(t => t.IpAddress)
                .HasColumnName("IpAddress")
                .HasConversion(
                    ip => ip == null ? null : ip.ToString(),
                    value => string.IsNullOrWhiteSpace(value) ? null : IPAddress.Parse(value));

            owned.HasKey("SetupSessionId", "MachineId");

            owned.HasIndex("MachineId");

            owned.OwnsOne(t => t.CallbackToken, token =>
            {
                token.WithOwner();

                token.Property(p => p.Token)
                    .HasColumnName("CallbackToken");

                token.Property(p => p.ExpiresAtUtc)
                    .HasColumnName("CallbackTokenExpiresAtUtc");

                token.Property(p => p.RevokedAtUtc)
                    .HasColumnName("CallbackTokenRevokedAtUtc");
            });
        });

        builder.Navigation(x => x.BootArtefactDescriptor).IsRequired(false);
        builder.Navigation(x => x.SshCredential).IsRequired(false);
    }
}
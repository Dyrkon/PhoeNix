using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
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

        builder.Property(x => x.OwnerId)
            .IsRequired()
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.HasIndex(x => x.OwnerId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.StartTime)
            .IsRequired();

        builder.OwnsOne(x => x.BootArtefactDescriptor, owned =>
        {
            owned.Property(p => p.Kernel)
                .HasColumnName("KernelLocation");

            owned.Property(p => p.RamDisk)
                .HasColumnName("InitRdLocation");

            owned.Property(p => p.Init)
                .HasColumnName("CmdLine");
        });

        builder.OwnsOne(x => x.SshCredential, owned =>
        {
            owned.Property(p => p.PublicKey)
                .HasColumnName("SshPublicKey");

            owned.Property(p => p.ExpiresAtUtc)
                .HasColumnName("SshKeyExpiresAtUtc");

            owned.Property(p => p.RevokedAtUtc)
                .HasColumnName("SshKeyRevokedAtUtc");

            owned.Property(p => p.CertificatePublicKey)
                .HasColumnName("SshCertificatePublicKey");
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

            owned.Property(t => t.LastTransitionAtUtc)
                .HasColumnName("LastTransitionAtUtc");

            owned.Property(t => t.LastErrorCode)
                .HasColumnName("LastErrorCode");

            owned.Property(t => t.LastErrorDescription)
                .HasColumnName("LastErrorDescription");

            owned.Property(t => t.LastErrorSource)
                .HasColumnName("LastErrorSource");

            owned.Property(t => t.LastErrorAtUtc)
                .HasColumnName("LastErrorAtUtc");

            owned.Property(t => t.IpAddress)
                .HasColumnName("IpAddress")
                .HasConversion(
                    ip => ip == null ? null : ip.ToString(),
                    value => string.IsNullOrWhiteSpace(value) ? null : IPAddress.Parse(value));

            owned.Property(t => t.SelectedSystemId)
                .HasColumnName("SelectedSystemId")
                .HasConversion(
                    id => id == null ? (Guid?)null : id.Value,
                    value => value == null ? null : new SystemId(value.Value));

            owned.Property(t => t.SelectedConfigurationId)
                .HasColumnName("SelectedConfigurationId")
                .HasConversion(
                    id => id == null ? (Guid?)null : id.Value,
                    value => value == null ? null : new ConfigurationId(value.Value));

            owned.HasKey("SetupSessionId", "MachineId");

            owned.HasIndex("MachineId");
            owned.HasIndex("SelectedSystemId");
            owned.HasIndex("SelectedConfigurationId");

            owned.OwnsOne(t => t.CallbackToken, token =>
            {
                token.Property(p => p.Token)
                    .HasColumnName("CallbackToken");

                token.Property(p => p.ExpiresAtUtc)
                    .HasColumnName("CallbackTokenExpiresAtUtc");

                token.Property(p => p.RevokedAtUtc)
                    .HasColumnName("CallbackTokenRevokedAtUtc");
            });

            owned.Navigation(t => t.CallbackToken)
                .IsRequired(false);

            owned.OwnsMany(t => t.RankedDiskAssignments, disk =>
            {
                disk.ToTable("SetupSessionTargetRankedDisks");

                disk.WithOwner()
                    .HasForeignKey("SetupSessionId", "MachineId");

                disk.Property(d => d.Index)
                    .HasColumnName("RankIndex")
                    .ValueGeneratedNever()
                    .IsRequired();

                disk.Property(d => d.DiskByIdPath)
                    .HasColumnName("DiskByIdPath")
                    .HasMaxLength(500)
                    .IsRequired();

                disk.HasKey("SetupSessionId", "MachineId", nameof(RankedDiskAssignment.Index));

                disk.HasIndex(d => d.DiskByIdPath);
            });

            owned.Navigation(t => t.RankedDiskAssignments)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.Navigation(x => x.BootArtefactDescriptor)
            .IsRequired(false);

        builder.Navigation(x => x.SshCredential)
            .IsRequired(false);

        builder.Navigation(x => x.Targets)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
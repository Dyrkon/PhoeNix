using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Entities.VmHosts;
using PhoeNix.Domain.Enums;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class VmHostEntityTypeConfiguration : IApplicationEntityTypeConfiguration<VmHost>
{
    public void Configure(EntityTypeBuilder<VmHost> builder)
    {
        builder.ToTable("VmHosts");

        builder.HasKey(i => i.Id);

        builder
            .Property(i => i.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => new VmHostId(value));

        builder
            .Property(i => i.OwnerId)
            .IsRequired()
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.HasIndex(i => i.OwnerId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder
            .HasIndex(i => i.Name)
            .IsUnique();

        builder
            .Property(i => i.Provider)
            .IsRequired()
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<VmHostProvider>(value))
            .HasMaxLength(32);

        builder
            .Property(i => i.Enabled)
            .IsRequired();

        builder
            .Property(i => i.LastSyncedAtUtc)
            .IsRequired(false);

        builder.OwnsOne(i => i.Credential, cred =>
        {
            cred.WithOwner();

            cred.Property(c => c.Host)
                .IsRequired()
                .HasMaxLength(512)
                .HasColumnName("CredentialHost");

            cred.Property(c => c.Port)
                .IsRequired(false)
                .HasColumnName("CredentialPort");

            cred.Property(c => c.Username)
                .IsRequired(false)
                .HasMaxLength(256)
                .HasColumnName("CredentialUsername");

            cred.Property(c => c.Secret)
                .IsRequired(false)
                .HasMaxLength(1024)
                .HasColumnName("CredentialSecret");

            cred.Property(c => c.ExtraConfig)
                .IsRequired(false)
                .HasMaxLength(4096)
                .HasColumnName("CredentialExtraConfig");
        });

        builder.OwnsOne(i => i.Resources, res =>
        {
            res.WithOwner();

            res.Property(r => r.TotalCpuCores).HasColumnName("ResourceTotalCpuCores");
            res.Property(r => r.UsedCpuCores).HasColumnName("ResourceUsedCpuCores");
            res.Property(r => r.TotalMemoryMb).HasColumnName("ResourceTotalMemoryMb");
            res.Property(r => r.UsedMemoryMb).HasColumnName("ResourceUsedMemoryMb");
            res.Property(r => r.TotalStorageGb).HasColumnName("ResourceTotalStorageGb");
            res.Property(r => r.UsedStorageGb).HasColumnName("ResourceUsedStorageGb");
        });

        builder.Navigation(i => i.Credential).IsRequired();
        builder.Navigation(i => i.Resources).IsRequired(false);
    }
}

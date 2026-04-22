using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class
    ConfigurationRevisionEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ConfigurationRevision>
{
    public void Configure(EntityTypeBuilder<ConfigurationRevision> builder)
    {
        builder.ToTable("ConfigurationRevisions");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => new ConfigurationId(value))
            .ValueGeneratedNever();

        builder.Property(r => r.ConfigurationId)
            .HasConversion(
                id => id.Value,
                value => new ConfigurationId(value))
            .IsRequired();

        builder.Property(r => r.SnapshotJson)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(r => r.Title)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired();
    }
}
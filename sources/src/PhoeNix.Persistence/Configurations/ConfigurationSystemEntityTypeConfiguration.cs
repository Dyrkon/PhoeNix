using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ConfigurationSystemEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ConfigurationSystem>
{
    public void Configure(EntityTypeBuilder<ConfigurationSystem> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasConversion(
            id => id.Value,
            value => new ConfigurationSystemId(value));

        builder.HasOne(c => c.Configuration)
            .WithMany(s => s.Systems)
            .HasForeignKey(c => c.ConfigurationId);

        builder.HasOne(s => s.System)
            .WithMany()
            .HasForeignKey(s => s.SystemId);
    }
}
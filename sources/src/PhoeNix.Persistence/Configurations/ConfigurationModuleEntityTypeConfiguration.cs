using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ConfigurationModuleEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ConfigurationModule>
{
    public void Configure(EntityTypeBuilder<ConfigurationModule> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasConversion(
            id => id.Value,
            value => new ConfigurationModuleId(value));

        builder.HasOne(c => c.Configuration)
            .WithMany(c => c.Modules)
            .HasForeignKey(c => c.ConfigurationId);

        builder.HasOne(c => c.Module)
            .WithMany()
            .HasForeignKey(m => m.ModuleId);
    }
}
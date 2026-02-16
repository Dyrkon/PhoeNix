using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal class ConfigurationEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Configuration>
{
    public void Configure(EntityTypeBuilder<Configuration> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => new ConfigurationId(value));

        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.Title).HasMaxLength(50);

        builder.HasMany(c => c.Inputs)
            .WithOne()
            .HasForeignKey(i => i.ConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Modules)
            .WithOne()
            .HasForeignKey("ConfigurationId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.SystemSpecifications)
            .WithOne()
            .HasForeignKey(s => s.ConfigurationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Inputs).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.Modules).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(c => c.SystemSpecifications).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
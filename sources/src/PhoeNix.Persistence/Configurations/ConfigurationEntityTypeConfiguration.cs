using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Homes;

namespace PhoeNix.Persistence.Configurations;

internal class ConfigurationEntityTypeConfiguration : IEntityTypeConfiguration<Configuration>
{
    public void Configure(EntityTypeBuilder<Configuration> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasConversion(
            configurationId => configurationId.Value,
            value => new ConfigurationId(value));

        builder.Property(c => c.Description).HasMaxLength(500);

        builder.Property(c => c.Title).HasMaxLength(50);
    }
}
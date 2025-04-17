using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;

namespace PhoeNix.Persistence.Configurations;

public class ConfigurationHomeEntityTypeConfiguration : IEntityTypeConfiguration<ConfigurationHome>
{
    public void Configure(EntityTypeBuilder<ConfigurationHome> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasConversion(
            id => id.Value,
            value => new ConfigurationHomeId(value));

        builder.HasOne(c => c.Configuration)
            .WithMany(c => c.Homes)
            .HasForeignKey(c => c.ConfigurationId);

        builder.HasOne(c => c.Home)
            .WithMany()
            .HasForeignKey(c => c.HomeId);
    }
}
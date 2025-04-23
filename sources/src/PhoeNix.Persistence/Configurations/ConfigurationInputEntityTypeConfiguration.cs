using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ConfigurationInputEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ConfigurationInput>
{
    public void Configure(EntityTypeBuilder<ConfigurationInput> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => new ConfigurationInputId(value));

        builder.HasOne(c => c.Configuration)
            .WithMany(c => c.Inputs)
            .HasForeignKey(c => c.ConfigurationId);

        builder.HasOne(i => i.Input)
            .WithMany()
            .HasForeignKey(i => i.InputId);
    }
}
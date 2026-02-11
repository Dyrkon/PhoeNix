using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class InputEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Input>
{
    public void Configure(EntityTypeBuilder<Input> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasConversion(
            id => id.Value,
            value => new InputId(value));

        builder.Property(i => i.ConfigurationId).HasConversion(
            id => id.Value,
            value => new ConfigurationId(value));

        builder.Property(i => i.Name).HasMaxLength(50);

        builder.Property(i => i.Source).HasMaxLength(500);

        builder.HasMany(i => i.Followers)
            .WithOne()
            .HasForeignKey(i => i.InputId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
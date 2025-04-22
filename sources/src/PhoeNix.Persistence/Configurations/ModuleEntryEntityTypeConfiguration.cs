using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ModuleEntryEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleEntry>
{
    public void Configure(EntityTypeBuilder<ModuleEntry> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasConversion(
            id => id.Value,
            value => new ModuleEntryId(value));

        builder.Property(m => m.Content).HasMaxLength(5000);
    }
}
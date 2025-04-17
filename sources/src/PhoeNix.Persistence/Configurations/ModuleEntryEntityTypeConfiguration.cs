using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Persistence.Configurations;

public class ModuleEntryEntityTypeConfiguration : IEntityTypeConfiguration<ModuleEntry>
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
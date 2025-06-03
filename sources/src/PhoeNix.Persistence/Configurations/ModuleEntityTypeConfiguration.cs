using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ModuleEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasConversion(
            id => id.Value,
            value => new ModuleId(value));

        builder.HasMany(m => m.EditableValues)
            .WithOne()
            .HasForeignKey(m => m.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
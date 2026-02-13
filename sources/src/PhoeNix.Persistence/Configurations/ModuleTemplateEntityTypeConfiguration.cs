using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ModuleTemplateEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleTemplate>
{
    public void Configure(EntityTypeBuilder<ModuleTemplate> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasConversion(
            id => id.Value,
            value => new ModuleTemplateId(value));

        builder.HasMany(m => m.EditableValueTypes)
            .WithOne()
            .HasForeignKey(m => m.ModuleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
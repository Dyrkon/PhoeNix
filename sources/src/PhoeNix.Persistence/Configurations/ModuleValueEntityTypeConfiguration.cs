using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ModuleValueEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleValue>
{
    public void Configure(EntityTypeBuilder<ModuleValue> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasConversion(
            id => id.Value,
            value => new ModuleValueId(value));

        builder.Property(c => c.ModuleTemplateId).HasConversion(
            id => id.Value, value => new ModuleTemplateId(value));

        builder.Property(c => c.ConfigurationId).HasConversion(
            id => id.Value, value => new ConfigurationId(value));

        builder.HasMany(m => m.EditableValues)
            .WithOne()
            .HasForeignKey(m => m.ModuleValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
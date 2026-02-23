using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal class ModuleTemplateEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleTemplate>
{
    public void Configure(EntityTypeBuilder<ModuleTemplate> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id).HasConversion(
            id => id.Value,
            value => new ModuleTemplateId(value));

        builder.OwnsMany(m => m.EditableValueTypes, b =>
        {
            b.WithOwner().HasForeignKey("ModuleTemplateId");
            b.Property<Guid>("Id");
            b.HasKey("Id");
            b.Property(e => e.ModuleTemplateId)
                .HasConversion(id => id.Value, value => new ModuleTemplateId(value));
            b.Property(e => e.Name).HasMaxLength(100);
            b.Property(e => e.Placeholder).HasMaxLength(100);
            b.Property(e => e.InputType);
        });

        builder.HasMany(m => m.Tests)
            .WithOne()
            .HasForeignKey(m => m.ModuleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
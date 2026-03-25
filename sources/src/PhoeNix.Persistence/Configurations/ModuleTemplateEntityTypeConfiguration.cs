using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class ModuleTemplateEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleTemplate>
{
    public void Configure(EntityTypeBuilder<ModuleTemplate> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => new ModuleTemplateId(value))
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.Type)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.Content)
            .IsRequired();

        builder.OwnsMany(m => m.EditableValueTypes, owned =>
        {
            owned.ToTable("ModuleTemplateEntryValueDefinitions");

            owned.WithOwner()
                .HasForeignKey("ModuleTemplateId");

            owned.Property<Guid>("Id");
            owned.HasKey("Id");

            owned.Property(e => e.ModuleTemplateId)
                .HasConversion(
                    id => id.Value,
                    value => new ModuleTemplateId(value));

            owned.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            owned.Property(e => e.Placeholder)
                .IsRequired()
                .HasMaxLength(100);

            owned.Property(e => e.InputType)
                .HasConversion<string>()
                .IsRequired();

            owned.Property(e => e.BindingKind)
                .HasConversion<string>()
                .IsRequired();

            owned.Property(e => e.BindingIndex);

            owned.HasIndex("ModuleTemplateId");
        });

        builder.PrimitiveCollection<List<Architecture>>("_supportedArchitectures")
            .ElementType()
            .HasConversion<string>();

        builder.HasMany(m => m.Tests)
            .WithOne()
            .HasForeignKey(t => t.ModuleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.EditableValueTypes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(m => m.Tests)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
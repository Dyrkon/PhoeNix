using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class ModuleTemplateEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleTemplate>
{
    public void Configure(EntityTypeBuilder<ModuleTemplate> builder)
    {
        builder.ToTable("ModuleTemplates");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => new ModuleTemplateId(value))
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(m => m.OwnerId)
            .IsRequired()
            .HasConversion(id => id.Value, value => new UserId(value));

        builder.HasIndex(m => m.OwnerId);

        builder.HasIndex(m => new { m.Name, m.OwnerId })
            .IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.Enabled)
            .IsRequired();

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
                    value => new ModuleTemplateId(value))
                .IsRequired();

            owned.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            owned.Property(e => e.Placeholder)
                .IsRequired()
                .HasMaxLength(100);

            owned.Property(e => e.BindingKind)
                .HasConversion<string>()
                .IsRequired();

            owned.Property(e => e.ValueKind)
                .HasConversion<string>()
                .IsRequired();

            owned.Property(e => e.DefaultValue);
            owned.Property(e => e.DefaultLowerValue);
            owned.Property(e => e.IntegerMin);
            owned.Property(e => e.IntegerMax);
            owned.Property(e => e.DecimalMin);
            owned.Property(e => e.DecimalMax);
            owned.Property(e => e.AllowLowerValue).IsRequired();
            owned.Property(e => e.OptionsJson);
            owned.Property(e => e.BindingIndex);

            owned.HasIndex("ModuleTemplateId");
            owned.HasIndex("ModuleTemplateId", nameof(EntryValueDefinition.Name)).IsUnique();
            owned.HasIndex("ModuleTemplateId", nameof(EntryValueDefinition.Placeholder)).IsUnique();
        });

        builder.PrimitiveCollection(m => m.SupportedArchitectures)
            .HasField("_supportedArchitectures")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .ElementType()
            .HasConversion<string>();

        builder.HasMany(m => m.Tests)
            .WithOne()
            .HasForeignKey(t => t.ModuleTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(m => m.EditableValueTypes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(m => m.RequiredInputs, owned =>
        {
            owned.ToTable("ModuleTemplateRequiredInputs");

            owned.WithOwner()
                .HasForeignKey("ModuleTemplateId");

            owned.Property<Guid>("Id");
            owned.HasKey("Id");

            owned.Property(e => e.ModuleTemplateId)
                .HasConversion(
                    id => id.Value,
                    value => new ModuleTemplateId(value))
                .IsRequired();

            owned.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            owned.Property(e => e.Source)
                .IsRequired();

            owned.HasIndex("ModuleTemplateId");
        });

        builder.Navigation(m => m.RequiredInputs)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(m => m.Tests)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
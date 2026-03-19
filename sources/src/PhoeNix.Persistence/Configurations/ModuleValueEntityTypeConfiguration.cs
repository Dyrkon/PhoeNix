using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class ModuleValueEntityTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleValue>
{
    public void Configure(EntityTypeBuilder<ModuleValue> builder)
    {
        builder.HasKey(mv => mv.Id);

        builder.Property(mv => mv.Id)
            .HasConversion(
                id => id.Value,
                value => new ModuleValueId(value))
            .ValueGeneratedNever();

        builder.Property(mv => mv.ModuleTemplateId)
            .HasConversion(
                id => id.Value,
                value => new ModuleTemplateId(value))
            .IsRequired();

        builder.Property(mv => mv.Enabled)
            .IsRequired();

        builder.Property<ConfigurationId?>("ConfigurationId")
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new ConfigurationId(value.Value));

        builder.Property<SystemId?>("SystemId")
            .HasConversion(
                id => id == null ? (Guid?)null : id.Value,
                value => value == null ? null : new SystemId(value.Value));

        builder.HasIndex("ConfigurationId");
        builder.HasIndex("SystemId");
        builder.HasIndex(mv => mv.ModuleTemplateId);

        builder.HasMany(mv => mv.EditableValues)
            .WithOne()
            .HasForeignKey(e => e.ModuleValueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(mv => mv.EditableValues)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
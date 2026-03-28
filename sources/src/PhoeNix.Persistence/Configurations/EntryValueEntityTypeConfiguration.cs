using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class EntryValueEntityTypeConfiguration : IApplicationEntityTypeConfiguration<EntryValue>
{
    public void Configure(EntityTypeBuilder<EntryValue> builder)
    {
        builder.ToTable("EntryValues");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(
                id => id.Value,
                value => new EntryValueId(value))
            .ValueGeneratedNever();

        builder.Property(e => e.ModuleValueId)
            .HasConversion(
                id => id.Value,
                value => new ModuleValueId(value))
            .IsRequired();

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Placeholder)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Value)
            .IsRequired();

        builder.HasIndex(e => e.ModuleValueId);

        builder.HasDiscriminator<string>("EntryValueKind")
            .HasValue<TextValue>(nameof(EntryValueKind.Text))
            .HasValue<IntegerRangeValue>(nameof(EntryValueKind.IntegerRange))
            .HasValue<DecimalRangeValue>(nameof(EntryValueKind.DecimalRange))
            .HasValue<SingleChoiceValue>(nameof(EntryValueKind.SingleChoice));
    }
}
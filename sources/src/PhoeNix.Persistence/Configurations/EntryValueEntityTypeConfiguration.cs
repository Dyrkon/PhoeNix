using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class EntryValueEntityTypeConfiguration : IApplicationEntityTypeConfiguration<EntryValue>
{
    public void Configure(EntityTypeBuilder<EntryValue> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new EntryValueId(value))
            .IsRequired();

        builder.Property(e => e.Name);
        builder.Property(e => e.Placeholder);
        builder.Property(e => e.Value);

        builder.Property(e => e.ModuleValueId)
            .HasConversion(id => id.Value, value => new ModuleValueId(value))
            .IsRequired();

        builder.Property<string>("TypeDiscriminator").HasColumnName("TypeDiscriminator");

        builder.HasDiscriminator<string>("TypeDiscriminator")
            .HasValue<TextValue>("Text")
            .HasValue<RangeValue<int>>("RangeInt")
            .HasValue<RangeValue<double>>("RangeDouble")
            .HasValue<MultiChoiceValue<string>>("MultiChoiceString")
            .HasValue<MultiChoiceValue<int>>("MultiChoiceInt")
            .HasValue<MultiChoiceValue<double>>("MultiChoiceDouble");
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class SingleChoiceValueEntityTypeConfiguration : IApplicationEntityTypeConfiguration<SingleChoiceValue>
{
    public void Configure(EntityTypeBuilder<SingleChoiceValue> builder)
    {
        builder.PrimitiveCollection(x => x.Options)
            .HasField("_options")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
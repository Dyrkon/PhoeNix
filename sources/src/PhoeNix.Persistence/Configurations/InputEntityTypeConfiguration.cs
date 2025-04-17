using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Inputs;

namespace PhoeNix.Persistence.Configurations;

public class InputEntityTypeConfiguration : IEntityTypeConfiguration<Input>
{
    public void Configure(EntityTypeBuilder<Input> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Id).HasConversion(
            id => id.Value,
            value => new InputId(value));

        builder.Property(i => i.Name).HasMaxLength(50);

        builder.Property(i => i.Source).HasMaxLength(500);
    }
}
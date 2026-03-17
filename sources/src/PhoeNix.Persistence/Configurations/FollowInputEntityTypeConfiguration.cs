using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal class FollowInputEntityTypeConfiguration : IApplicationEntityTypeConfiguration<FollowInput>
{
    public void Configure(EntityTypeBuilder<FollowInput> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.InputId)
            .HasConversion(id => id.Value, value => new InputId(value))
            .IsRequired();

        builder.Property(f => f.FollowName).HasMaxLength(64);
        builder.Property(f => f.FollowValue).HasMaxLength(64);
    }
}
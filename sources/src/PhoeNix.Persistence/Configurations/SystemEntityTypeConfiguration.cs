using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Persistence.Configurations;

public class SystemEntityTypeConfiguration : IEntityTypeConfiguration<Domain.Entities.Systems.System>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Systems.System> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasConversion(
            id => id.Value,
            value => new SystemId(value));

        builder.Property(s => s.Name).HasMaxLength(50);
    }
}
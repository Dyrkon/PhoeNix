using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class SystemEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Domain.Entities.Systems.System>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Systems.System> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new SystemId(value));

        builder.Property(s => s.ConfigurationId)
            .HasConversion(id => id.Value, value => new ConfigurationId(value));

        builder.Property(s => s.Name).HasMaxLength(50);

        builder.HasMany(s => s.Modules)
            .WithOne()
            .HasForeignKey("SystemId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(s => s.Modules).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Systems;

namespace PhoeNix.Persistence.Configurations;

public class SystemModuleEntityTypeConfiguration : IEntityTypeConfiguration<SystemModule>
{
    public void Configure(EntityTypeBuilder<SystemModule> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasConversion(
            id => id.Value,
            value => new SystemModuleId(value));

        builder.HasOne(s => s.System)
            .WithMany(m => m.Modules)
            .HasForeignKey(s => s.SystemId);

        builder.HasOne(m => m.Module)
            .WithMany()
            .HasForeignKey(m => m.ModuleId);
    }
}
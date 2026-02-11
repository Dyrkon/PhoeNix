using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class ModuleTestTypeConfiguration : IApplicationEntityTypeConfiguration<ModuleTest>
{
    public void Configure(EntityTypeBuilder<ModuleTest> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id).HasConversion(id => id.Value, id => new ModuleTestId(id));

        builder.HasOne(m => m.Module).WithMany(m => m.Tests).HasForeignKey(m => m.ModuleId);

        builder.HasOne(m => m.Test).WithMany().HasForeignKey(m => m.TestId);
    }
}
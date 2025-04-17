using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Homes;

namespace PhoeNix.Persistence.Configurations;

public class HomeModuleEntityTypeConfiguration : IEntityTypeConfiguration<HomeModule>
{
    public void Configure(EntityTypeBuilder<HomeModule> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id).HasConversion(
            id => id.Value,
            value => new HomeModuleId(value));

        builder.HasOne(h => h.Home)
            .WithMany(h => h.Modules)
            .HasForeignKey(h => h.HomeId);

        builder.HasOne(m => m.Module)
            .WithMany()
            .HasForeignKey(m => m.ModuleId);
    }
}
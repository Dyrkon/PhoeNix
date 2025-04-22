using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public class HomeUserEntityTypeConfiguration : IApplicationEntityTypeConfiguration<HomeUser>
{
    public void Configure(EntityTypeBuilder<HomeUser> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Id).HasConversion(
            id => id.Value,
            value => new HomeUserId(value));

        builder.HasOne(h => h.Home)
            .WithMany(h => h.Users)
            .HasForeignKey(h => h.HomeId);

        builder.HasOne(u => u.User)
            .WithMany()
            .HasForeignKey(u => u.UserId);
    }
}
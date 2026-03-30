using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class UserEntityTypeConfiguration : IApplicationEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.NormalizedName)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        builder.HasIndex(x => x.NormalizedName)
            .IsUnique();

        builder.Property(x => x.Id).HasConversion(id => id.Value, value => new UserId(value));
    }
}
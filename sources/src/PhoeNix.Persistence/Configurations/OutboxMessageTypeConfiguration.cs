using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Application.Models.Outbox;
using PhoeNix.Persistence.Configurations.Abstractions;
using PhoeNix.Persistence.Outbox;

namespace PhoeNix.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>,
    IApplicationEntityTypeConfiguration
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Type)
            .IsRequired();

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.Error);

        builder.HasIndex(x => new { x.ProcessedOnUtc, x.NextAttemptOnUtc, x.OccurredOnUtc });
    }
}
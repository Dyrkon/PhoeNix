using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal class TestTypeConfiguration : IApplicationEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.HasKey(x => x.TemplateId);

        builder.Property(t => t.TemplateId).HasConversion(
            testId => testId.Value, value => new ModuleTemplateId(value));

        builder.Property(t => t.Name).HasMaxLength(100);

        builder.Property(t => t.Content).HasMaxLength(10000);
    }
}
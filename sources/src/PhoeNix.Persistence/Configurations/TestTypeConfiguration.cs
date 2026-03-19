using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class TestEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => new TestId(value))
            .ValueGeneratedNever();

        builder.Property(t => t.ModuleTemplateId)
            .HasConversion(
                id => id.Value,
                value => new ModuleTemplateId(value))
            .IsRequired();

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Content)
            .IsRequired();

        builder.PrimitiveCollection<List<string>>("_variableNames")
            .ElementType()
            .HasMaxLength(200);
    }
}
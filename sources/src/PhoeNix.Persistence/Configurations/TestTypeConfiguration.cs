using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class TestEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.ToTable("ModuleTemplateTests");

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

        builder.HasIndex(t => t.ModuleTemplateId);

        builder.HasIndex(t => new { t.ModuleTemplateId, t.Name })
            .IsUnique();

        builder.PrimitiveCollection(t => t.VariableNames)
            .HasField("_variableNames")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
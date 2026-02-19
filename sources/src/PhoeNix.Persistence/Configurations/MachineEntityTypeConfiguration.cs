using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

public sealed class MachineEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Machine>
{
    public void Configure(EntityTypeBuilder<Machine> builder)
    {
        builder.HasKey(i => i.Id);

        builder
            .Property(i => i.Id)
            .ValueGeneratedNever()
            .HasConversion(id => id.Value, value => new MachineId(value));

        builder
            .Property(i => i.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder
            .HasIndex(i => i.Title)
            .IsUnique();

        builder
            .Property(i => i.Enabled)
            .IsRequired();

        builder
            .Property(i => i.MacAddress)
            .IsRequired()
            .HasMaxLength(12)
            .HasConversion(
                mac => mac.ToString(),
                s => PhysicalAddress.Parse(NormalizeMac(s)));

        builder
            .HasIndex(i => i.MacAddress)
            .IsUnique();

        builder.OwnsOne(i => i.MachineStatus, owned =>
        {
            owned.Property(s => s.MachineState)
                .IsRequired()
                .HasConversion(
                    v => v.ToString(),
                    v => Enum.Parse<MachineState>(v))
                .HasMaxLength(32);

            owned.Property(s => s.LastContacted)
                .IsRequired(false);

            owned.Property(s => s.LastProvisioned)
                .IsRequired(false);

            owned.Property(s => s.LastOrchestrated)
                .IsRequired(false);

            owned.Property(s => s.LastConfigured)
                .IsRequired(false);

            owned.HasIndex(s => s.MachineState);
        });

        builder.OwnsOne(i => i.HardwareProfile);
        builder.OwnsOne(i => i.SoftwareSnapshot);

        builder.Navigation(i => i.MachineStatus).IsRequired();
        builder.Navigation(i => i.HardwareProfile).IsRequired();
        builder.Navigation(i => i.SoftwareSnapshot).IsRequired();
    }

    private static string NormalizeMac(string input)
    {
        var cleaned = input
            .Trim()
            .Replace(":", string.Empty)
            .Replace("-", string.Empty)
            .Replace(".", string.Empty)
            .ToUpperInvariant();

        if (cleaned.Length != 12)
            throw new FormatException($"Invalid MAC address '{input}'.");

        return cleaned;
    }
}
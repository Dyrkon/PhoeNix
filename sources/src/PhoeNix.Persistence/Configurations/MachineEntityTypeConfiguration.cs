using System.Net;
using System.Net.NetworkInformation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Persistence.Configurations.Abstractions;

namespace PhoeNix.Persistence.Configurations;

internal sealed class MachineEntityTypeConfiguration : IApplicationEntityTypeConfiguration<Machine>
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
            .Property(i => i.Architecture)
            .IsRequired()
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<Architecture>(value))
            .HasMaxLength(32);

        builder
            .Property(i => i.InstallDiskSelectionPreference)
            .IsRequired()
            .HasConversion(
                value => value.ToString(),
                value => Enum.Parse<InstallDiskSelectionPreference>(value))
            .HasMaxLength(32);

        builder
            .Property(i => i.MacAddress)
            .IsRequired()
            .HasMaxLength(12)
            .HasConversion(
                mac => mac.ToString(),
                value => PhysicalAddress.Parse(NormalizeMac(value)));

        builder
            .HasIndex(i => i.MacAddress)
            .IsUnique();

        builder.OwnsOne(i => i.MachineStatus, owned =>
        {
            owned.Property(s => s.MachineState)
                .IsRequired()
                .HasConversion(
                    value => value.ToString(),
                    value => Enum.Parse<MachineState>(value))
                .HasMaxLength(32)
                .HasColumnName("MachineState");

            owned.Property(s => s.LastContacted)
                .IsRequired(false)
                .HasColumnName("LastContacted");

            owned.Property(s => s.LastProvisioned)
                .IsRequired(false)
                .HasColumnName("LastProvisioned");

            owned.Property(s => s.LastOrchestrated)
                .IsRequired(false)
                .HasColumnName("LastOrchestrated");

            owned.Property(s => s.LastConfigured)
                .IsRequired(false)
                .HasColumnName("LastConfigured");

            owned.HasIndex(s => s.MachineState);
        });

        builder.OwnsOne(i => i.HardwareProfile, hardware =>
        {
            hardware.WithOwner();

            hardware.Property(p => p.ObservedAtUtc)
                .IsRequired()
                .HasColumnName("HardwareObservedAtUtc");

            hardware.OwnsOne(p => p.Cpu, cpu =>
            {
                cpu.Property(c => c.Vendor)
                    .HasMaxLength(200)
                    .HasColumnName("CpuVendor");

                cpu.Property(c => c.Model)
                    .HasMaxLength(300)
                    .HasColumnName("CpuModel");

                cpu.Property(c => c.CoreCount)
                    .HasColumnName("CpuCoreCount");

                cpu.Property(c => c.ThreadCount)
                    .HasColumnName("CpuThreadCount");
            });

            hardware.OwnsOne(p => p.Motherboard, board =>
            {
                board.Property(b => b.Vendor)
                    .HasMaxLength(200)
                    .HasColumnName("MotherboardVendor");

                board.Property(b => b.Model)
                    .HasMaxLength(300)
                    .HasColumnName("MotherboardModel");
            });

            hardware.OwnsOne(p => p.Memory, memory =>
            {
                memory.Property(m => m.TotalBytes)
                    .HasColumnName("MemoryTotalBytes");

                memory.Property(m => m.SlotCount)
                    .HasColumnName("MemorySlotCount");

                memory.Property(m => m.OccupiedSlotCount)
                    .HasColumnName("MemoryOccupiedSlotCount");

                memory.OwnsMany(m => m.Modules, module =>
                {
                    module.ToTable("MachineMemoryModules");

                    module.WithOwner().HasForeignKey("MachineId");

                    module.Property<int>("Id");
                    module.HasKey("Id");

                    module.Property(m => m.Slot)
                        .HasMaxLength(100);

                    module.Property(m => m.SizeBytes);
                });
            });

            hardware.OwnsMany(p => p.Gpus, gpu =>
            {
                gpu.ToTable("MachineGpus");

                gpu.WithOwner().HasForeignKey("MachineId");

                gpu.Property<int>("Id");
                gpu.HasKey("Id");

                gpu.Property(g => g.Vendor)
                    .HasMaxLength(200);

                gpu.Property(g => g.Model)
                    .HasMaxLength(300);

                gpu.Property(g => g.VramBytes);
            });

            hardware.OwnsMany(p => p.Disks, disk =>
            {
                disk.ToTable("MachineDisks");

                disk.WithOwner().HasForeignKey("MachineId");

                disk.Property<int>("Id");
                disk.HasKey("Id");

                disk.Property(d => d.StableDevicePath)
                    .HasMaxLength(500);

                disk.Property(d => d.KernelDevicePath)
                    .HasMaxLength(200);

                disk.Property(d => d.Model)
                    .HasMaxLength(300);

                disk.Property(d => d.Vendor)
                    .HasMaxLength(200);

                disk.Property(d => d.BusType)
                    .HasMaxLength(100);

                disk.Property(d => d.SizeBytes);

                disk.Property(d => d.IsRotational);
            });

            hardware.OwnsMany(p => p.Peripherals, peripheral =>
            {
                peripheral.ToTable("MachinePeripherals");

                peripheral.WithOwner().HasForeignKey("MachineId");

                peripheral.Property<int>("Id");
                peripheral.HasKey("Id");

                peripheral.Property(p => p.Kind)
                    .IsRequired()
                    .HasConversion(
                        value => value.ToString(),
                        value => Enum.Parse<PeripheralKind>(value))
                    .HasMaxLength(64);

                peripheral.Property(p => p.Name)
                    .HasMaxLength(300);

                peripheral.Property(p => p.IsConnected)
                    .IsRequired();
            });
        });

        builder.OwnsOne(i => i.DeploymentSnapshot, deployment =>
        {
            deployment.WithOwner();

            deployment.Property(p => p.ConfigurationId)
                .HasColumnName("ProvisionedConfigurationId")
                .HasConversion(
                    id => id.Value,
                    value => new ConfigurationId(value));

            deployment.Property(p => p.SystemId)
                .HasColumnName("ProvisionedSystemId")
                .HasConversion(
                    id => id.Value,
                    value => new SystemId(value));

            deployment.Property(p => p.LastKnownIpAddress)
                .HasColumnName("ProvisionedIpAddress")
                .HasConversion(
                    ip => ip.ToString(),
                    value => IPAddress.Parse(value));

            deployment.Property(p => p.ProvisionedAtUtc)
                .HasColumnName("ProvisionedAtUtc");

            deployment.OwnsMany(p => p.BoundDisks, disk =>
            {
                disk.ToTable("MachineDeploymentBoundDisks");

                disk.WithOwner().HasForeignKey("MachineId");

                disk.Property(d => d.Index)
                    .HasColumnName("DiskIndex")
                    .ValueGeneratedNever()
                    .IsRequired();

                disk.Property(d => d.StableDevicePath)
                    .HasColumnName("StableDevicePath")
                    .HasMaxLength(500)
                    .IsRequired();

                disk.HasKey("MachineId", nameof(DeploymentDiskBinding.Index));

                disk.HasIndex(d => d.StableDevicePath);
            });

            deployment.Navigation(p => p.BoundDisks)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        builder.OwnsOne(i => i.SoftwareSnapshot, software => { software.WithOwner(); });

        builder.Navigation(i => i.DeploymentSnapshot).IsRequired(false);
        builder.Navigation(i => i.HardwareProfile).IsRequired(false);
        builder.Navigation(i => i.SoftwareSnapshot).IsRequired(false);
        builder.Navigation(i => i.MachineStatus).IsRequired();
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
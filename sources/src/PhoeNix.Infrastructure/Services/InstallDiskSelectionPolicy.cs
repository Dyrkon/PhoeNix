using PhoeNix.Application.Abstractions.Setup;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Infrastructure.Services;

public sealed class InstallDiskSelectionPolicy : IInstallDiskSelectionPolicy
{
    public Result<DiskProfile> Select(
        IReadOnlyCollection<DiskProfile> disks,
        InstallDiskSelectionPreference preference)
    {
        if (disks.Count == 0)
            return Result.Failure<DiskProfile>(new Error(
                "InstallDiskSelectionNoDisks",
                "No disks were provided for installation target selection."));

        var candidates = disks
            .Where(IsSelectable)
            .ToList();

        if (candidates.Count == 0)
            return Result.Failure<DiskProfile>(new Error(
                "InstallDiskSelectionNoCandidates",
                "No selectable installation target disks were found."));

        var ordered = preference switch
        {
            InstallDiskSelectionPreference.Biggest =>
                candidates
                    .OrderByDescending(GetSizeBytes)
                    .ThenByDescending(GetSpeedScore)
                    .ThenBy(GetStableIdentity),

            InstallDiskSelectionPreference.Fastest =>
                candidates
                    .OrderByDescending(GetSpeedScore)
                    .ThenByDescending(GetSizeBytes)
                    .ThenBy(GetStableIdentity),

            InstallDiskSelectionPreference.FastestAndBiggest =>
                candidates
                    .OrderByDescending(GetSpeedScore)
                    .ThenByDescending(GetSizeBytes)
                    .ThenBy(GetStableIdentity),

            InstallDiskSelectionPreference.BiggestAndFastest =>
                candidates
                    .OrderByDescending(GetSizeBytes)
                    .ThenByDescending(GetSpeedScore)
                    .ThenBy(GetStableIdentity),

            _ => throw new ArgumentOutOfRangeException(nameof(preference), preference, null)
        };

        var selected = ordered.First();

        return Result.Success(selected);
    }

    private static bool IsSelectable(DiskProfile disk)
    {
        return !string.IsNullOrWhiteSpace(disk.StableDevicePath)
               && GetSizeBytes(disk) > 0;
    }

    private static long GetSizeBytes(DiskProfile disk)
    {
        return disk.SizeBytes ?? 0;
    }

    private static string GetStableIdentity(DiskProfile disk)
    {
        return disk.StableDevicePath
               ?? disk.KernelDevicePath
               ?? disk.Model
               ?? string.Empty;
    }

    private static int GetSpeedScore(DiskProfile disk)
    {
        var bus = Normalize(disk.BusType);
        var model = Normalize(disk.Model);
        var rotational = disk.IsRotational;

        if (Contains(bus, "nvme") || Contains(model, "nvme"))
            return 400;

        if (rotational == false)
        {
            if (Contains(bus, "pcie"))
                return 350;

            if (Contains(bus, "sata") || Contains(bus, "ata") || Contains(bus, "ahci"))
                return 300;

            if (Contains(bus, "scsi"))
                return 280;

            return 250;
        }

        if (rotational == true)
        {
            if (Contains(bus, "sata") || Contains(bus, "ata") || Contains(bus, "ahci"))
                return 150;

            if (Contains(bus, "scsi"))
                return 140;

            return 120;
        }

        if (Contains(bus, "usb"))
            return 50;

        return 100;
    }

    private static string Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToLowerInvariant();
    }

    private static bool Contains(string source, string value)
    {
        return source.Contains(value, StringComparison.Ordinal);
    }
}
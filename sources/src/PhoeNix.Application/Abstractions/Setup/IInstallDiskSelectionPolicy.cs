using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Setup;

public interface IInstallDiskSelectionPolicy
{
    Result<DiskProfile> Select(
        IReadOnlyCollection<DiskProfile> disks,
        InstallDiskSelectionPreference preference);
}
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Application.Abstractions.Setup;

public interface IInstallDiskSelectionPolicy
{
    Result<IReadOnlyList<DiskProfile>> Rank(
        IReadOnlyCollection<DiskProfile> disks,
        InstallDiskSelectionPreference preference);
}
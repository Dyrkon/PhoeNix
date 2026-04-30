using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.Entities.SetupSessions;

public sealed class RankedDiskAssignment
{
    private RankedDiskAssignment()
    {
    }

    public int Index { get; private set; }

    public string DiskByIdPath { get; private set; }

    public Result UpdateDiskByIdPath(string newPath)
    {
        if (newPath == DiskByIdPath)
            return Result.Failure(new Error("DiskByIdPathAlreadyAssigned",
                $"Disk path {newPath} is already assigned."));

        DiskByIdPath = newPath;
        return Result.Success();
    }

    public static Result<RankedDiskAssignment> Create(int index, string diskByIdPath)
    {
        if (index < 0)
            return Result.Failure<RankedDiskAssignment>(new Error(
                "RankedDiskAssignmentIndexInvalid",
                "Ranked disk assignment index must be zero or greater."));

        if (string.IsNullOrWhiteSpace(diskByIdPath))
            return Result.Failure<RankedDiskAssignment>(new Error(
                "RankedDiskAssignmentPathInvalid",
                "Ranked disk assignment path cannot be empty."));

        if (!diskByIdPath.StartsWith("/dev/disk/by-id/", StringComparison.Ordinal))
            return Result.Failure<RankedDiskAssignment>(new Error(
                "RankedDiskAssignmentPathInvalid",
                $"Disk path '{diskByIdPath}' must be a stable /dev/disk/by-id/... path."));

        return Result.Success(new RankedDiskAssignment
        {
            Index = index,
            DiskByIdPath = diskByIdPath.Trim()
        });
    }
}
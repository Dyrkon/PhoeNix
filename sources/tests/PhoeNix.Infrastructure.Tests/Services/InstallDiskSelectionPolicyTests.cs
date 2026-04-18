using FluentAssertions;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Enums;
using PhoeNix.Infrastructure.Services.HardwareManagement;

namespace PhoeNix.Infrastructure.Tests.Services;

public class InstallDiskSelectionPolicyTests
{
    private readonly InstallDiskSelectionPolicy _sut = new();

    private static DiskProfile MakeDisk(
        string stablePath,
        long sizeBytes,
        string? busType = null,
        bool? isRotational = null,
        string? model = null)
        => DiskProfile.Create(stablePath, "/dev/sda", model, null, busType, sizeBytes, isRotational);

    [Fact]
    public void Rank_Should_Fail_When_No_Disks()
    {
        var result = _sut.Rank(Array.Empty<DiskProfile>(), InstallDiskSelectionPreference.Biggest);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InstallDiskSelectionNoDisks");
    }

    [Fact]
    public void Rank_Should_Fail_When_No_Selectable_Candidates()
    {
        var disk = DiskProfile.Create(null, "/dev/sda", null, null, null, 100L, null);

        var result = _sut.Rank(new[] { disk }, InstallDiskSelectionPreference.Biggest);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InstallDiskSelectionNoCandidates");
    }

    [Fact]
    public void Rank_Should_Fail_When_Disk_Has_No_Stable_Path()
    {
        var disk = DiskProfile.Create(null, "/dev/sda", "MySSD", null, "SATA", 1000L, false);

        var result = _sut.Rank(new[] { disk }, InstallDiskSelectionPreference.Fastest);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("InstallDiskSelectionNoCandidates");
    }

    [Fact]
    public void Rank_Biggest_Should_Order_By_Size_Descending()
    {
        var small = MakeDisk("/dev/disk/by-id/small", 100L, "SATA", false);
        var large = MakeDisk("/dev/disk/by-id/large", 10000L, "SATA", false);
        var medium = MakeDisk("/dev/disk/by-id/medium", 5000L, "SATA", false);

        var result = _sut.Rank(new[] { small, large, medium }, InstallDiskSelectionPreference.Biggest);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].StableDevicePath.Should().Be("/dev/disk/by-id/large");
        result.Value[1].StableDevicePath.Should().Be("/dev/disk/by-id/medium");
        result.Value[2].StableDevicePath.Should().Be("/dev/disk/by-id/small");
    }

    [Fact]
    public void Rank_Fastest_Should_Order_NVMe_First()
    {
        var hdd = MakeDisk("/dev/disk/by-id/hdd", 10000L, "SATA", true);
        var ssd = MakeDisk("/dev/disk/by-id/ssd", 5000L, "SATA", false);
        var nvme = MakeDisk("/dev/disk/by-id/nvme-0", 2000L, "NVMe", false);

        var result = _sut.Rank(new[] { hdd, ssd, nvme }, InstallDiskSelectionPreference.Fastest);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].StableDevicePath.Should().Be("/dev/disk/by-id/nvme-0");
        result.Value[1].StableDevicePath.Should().Be("/dev/disk/by-id/ssd");
        result.Value[2].StableDevicePath.Should().Be("/dev/disk/by-id/hdd");
    }

    [Fact]
    public void Rank_BiggestAndFastest_Should_Order_By_Size_Then_Speed()
    {
        var bigHdd = MakeDisk("/dev/disk/by-id/big-hdd", 20000L, "SATA", true);
        var bigSsd = MakeDisk("/dev/disk/by-id/big-ssd", 20000L, "SATA", false);

        var result = _sut.Rank(new[] { bigHdd, bigSsd }, InstallDiskSelectionPreference.BiggestAndFastest);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].StableDevicePath.Should().Be("/dev/disk/by-id/big-ssd");
    }

    [Fact]
    public void Rank_FastestAndBiggest_Should_Order_By_Speed_Then_Size()
    {
        var smallNvme = MakeDisk("/dev/disk/by-id/small-nvme", 1000L, "NVMe", false);
        var bigSsd = MakeDisk("/dev/disk/by-id/big-ssd", 50000L, "SATA", false);

        var result = _sut.Rank(new[] { bigSsd, smallNvme }, InstallDiskSelectionPreference.FastestAndBiggest);

        result.IsSuccess.Should().BeTrue();
        result.Value[0].StableDevicePath.Should().Be("/dev/disk/by-id/small-nvme");
    }

    [Fact]
    public void Rank_Should_Exclude_Disk_With_Zero_Size()
    {
        var zeroDisk = DiskProfile.Create("/dev/disk/by-id/zero", null, null, null, null, 0L, null);
        var goodDisk = MakeDisk("/dev/disk/by-id/good", 5000L, "SATA", false);

        var result = _sut.Rank(new[] { zeroDisk, goodDisk }, InstallDiskSelectionPreference.Biggest);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        result.Value[0].StableDevicePath.Should().Be("/dev/disk/by-id/good");
    }
}

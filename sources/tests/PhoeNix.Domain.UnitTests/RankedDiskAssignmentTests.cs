using FluentAssertions;
using PhoeNix.Domain.Entities.SetupSessions;

namespace PhoeNix.Domain.UnitTests;

public class RankedDiskAssignmentTests
{
    [Fact]
    public void RankedDiskAssignment_Should_Create_Successfully()
    {
        var result = RankedDiskAssignment.Create(0, "/dev/disk/by-id/nvme-Samsung_980");

        result.IsSuccess.Should().BeTrue();
        result.Value.Index.Should().Be(0);
        result.Value.DiskByIdPath.Should().Be("/dev/disk/by-id/nvme-Samsung_980");
    }

    [Fact]
    public void RankedDiskAssignment_Should_Fail_When_Index_Negative()
    {
        var result = RankedDiskAssignment.Create(-1, "/dev/disk/by-id/disk");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("zero or greater");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void RankedDiskAssignment_Should_Fail_When_Path_Empty(string path)
    {
        var result = RankedDiskAssignment.Create(0, path);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("cannot be empty");
    }

    [Fact]
    public void RankedDiskAssignment_Should_Fail_When_Path_Not_By_Id()
    {
        var result = RankedDiskAssignment.Create(0, "/dev/sda");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("/dev/disk/by-id/");
    }

    [Fact]
    public void RankedDiskAssignment_Should_Create_Multiple_With_Sequential_Indexes()
    {
        var r0 = RankedDiskAssignment.Create(0, "/dev/disk/by-id/disk-a");
        var r1 = RankedDiskAssignment.Create(1, "/dev/disk/by-id/disk-b");

        r0.IsSuccess.Should().BeTrue();
        r1.IsSuccess.Should().BeTrue();
        r0.Value.Index.Should().Be(0);
        r1.Value.Index.Should().Be(1);
    }
}

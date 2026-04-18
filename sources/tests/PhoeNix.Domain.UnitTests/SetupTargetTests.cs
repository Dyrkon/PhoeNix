using System.Net;
using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.UnitTests;

public class SetupTargetTests
{
    private readonly MachineId _machineId = new(Guid.NewGuid());
    private readonly SystemId _systemId = new(Guid.NewGuid());
    private readonly ConfigurationId _configId = new(Guid.NewGuid());
    private readonly DateTime _now = DateTime.UtcNow;

    private SetupTarget CreateTarget() =>
        SetupTarget.Create(_machineId, _systemId, _configId, _now).Value;

    [Fact]
    public void SetupTarget_Should_Create_Successfully()
    {
        var result = SetupTarget.Create(_machineId, _systemId, _configId, _now);

        result.IsSuccess.Should().BeTrue();
        result.Value.MachineId.Should().Be(_machineId);
        result.Value.SelectedSystemId.Should().Be(_systemId);
        result.Value.SelectedConfigurationId.Should().Be(_configId);
        result.Value.Stage.Should().Be(SetupStage.Created);
        result.Value.LastTransitionAtUtc.Should().Be(_now);
        result.Value.CallbackToken.Should().BeNull();
        result.Value.IpAddress.Should().BeNull();
        result.Value.RankedDiskAssignments.Should().BeEmpty();
    }

    [Fact]
    public void SetupTarget_Should_SetStage()
    {
        var target = CreateTarget();
        var later = _now.AddMinutes(5);

        var result = target.SetStage(SetupStage.WaitingForPxe, later);

        result.IsSuccess.Should().BeTrue();
        target.Stage.Should().Be(SetupStage.WaitingForPxe);
        target.LastTransitionAtUtc.Should().Be(later);
    }

    [Fact]
    public void SetupTarget_Should_SetStage_With_ClearFailure()
    {
        var target = CreateTarget();
        target.RecordFailure(new Error("E", "desc"), "src", _now);

        target.SetStage(SetupStage.Probed, _now, clearFailure: true);

        target.LastErrorCode.Should().BeNull();
    }

    [Fact]
    public void SetupTarget_Should_SetStage_Without_ClearFailure()
    {
        var target = CreateTarget();
        target.RecordFailure(new Error("E", "desc"), "src", _now);

        target.SetStage(SetupStage.Probed, _now, clearFailure: false);

        target.LastErrorCode.Should().Be("E");
    }

    [Fact]
    public void SetupTarget_Should_RecordFailure()
    {
        var target = CreateTarget();
        var error = new Error("Modules.SomeError", "Something broke");

        var result = target.RecordFailure(error, "my-service", _now);

        result.IsSuccess.Should().BeTrue();
        target.LastErrorCode.Should().Be("Modules.SomeError");
        target.LastErrorDescription.Should().Be("Something broke");
        target.LastErrorSource.Should().Be("my-service");
        target.LastErrorAtUtc.Should().Be(_now);
    }

    [Fact]
    public void SetupTarget_Should_ClearFailure()
    {
        var target = CreateTarget();
        target.RecordFailure(new Error("E", "desc"), "src", _now);

        var result = target.ClearFailure();

        result.IsSuccess.Should().BeTrue();
        target.LastErrorCode.Should().BeNull();
        target.LastErrorDescription.Should().BeNull();
        target.LastErrorSource.Should().BeNull();
        target.LastErrorAtUtc.Should().BeNull();
    }

    [Fact]
    public void SetupTarget_Should_SetIpAddress()
    {
        var target = CreateTarget();
        var ip = IPAddress.Parse("192.168.0.50");

        var result = target.SetIpAddress(ip);

        result.IsSuccess.Should().BeTrue();
        target.IpAddress.Should().Be(ip);
    }

    [Fact]
    public void SetupTarget_Should_AssignRankedDisks()
    {
        var target = CreateTarget();
        var paths = new[] { "/dev/disk/by-id/disk-a", "/dev/disk/by-id/disk-b" };

        var result = target.AssignRankedDisks(paths);

        result.IsSuccess.Should().BeTrue();
        target.RankedDiskAssignments.Should().HaveCount(2);
        target.RankedDiskAssignments.First().Index.Should().Be(0);
        target.RankedDiskAssignments.First().DiskByIdPath.Should().Be("/dev/disk/by-id/disk-a");
        target.RankedDiskAssignments.Last().Index.Should().Be(1);
    }

    [Fact]
    public void SetupTarget_Should_Fail_AssignRankedDisks_When_Empty()
    {
        var target = CreateTarget();

        var result = target.AssignRankedDisks(Array.Empty<string>());

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("At least one");
    }

    [Fact]
    public void SetupTarget_Should_Replace_RankedDisks_On_Reassign()
    {
        var target = CreateTarget();
        target.AssignRankedDisks(new[] { "/dev/disk/by-id/old" });

        target.AssignRankedDisks(new[] { "/dev/disk/by-id/new1", "/dev/disk/by-id/new2" });

        target.RankedDiskAssignments.Should().HaveCount(2);
        target.RankedDiskAssignments.Should().NotContain(d => d.DiskByIdPath == "/dev/disk/by-id/old");
    }

    [Fact]
    public void SetupTarget_Should_ClearRankedDisks()
    {
        var target = CreateTarget();
        target.AssignRankedDisks(new[] { "/dev/disk/by-id/sda" });

        var result = target.ClearRankedDisks();

        result.IsSuccess.Should().BeTrue();
        target.RankedDiskAssignments.Should().BeEmpty();
    }

    [Fact]
    public void SetupTarget_Should_AssignToken()
    {
        var target = CreateTarget();
        var token = new CallbackToken("abc123", _now.AddHours(1), null);

        var result = target.AssignToken(token);

        result.IsSuccess.Should().BeTrue();
        target.CallbackToken.Should().NotBeNull();
        target.CallbackToken!.Token.Should().Be("abc123");
    }

    [Fact]
    public void SetupTarget_Should_Fail_AssignToken_When_Already_Assigned()
    {
        var target = CreateTarget();
        target.AssignToken(new CallbackToken("first", _now.AddHours(1), null));

        var result = target.AssignToken(new CallbackToken("second", _now.AddHours(1), null));

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already assigned");
    }

    [Fact]
    public void SetupTarget_Should_ClearCallbackToken()
    {
        var target = CreateTarget();
        target.AssignToken(new CallbackToken("tok", _now.AddHours(1), null));

        var result = target.ClearCallbackToken();

        result.IsSuccess.Should().BeTrue();
        target.CallbackToken.Should().BeNull();
    }

    [Fact]
    public void SetupTarget_Should_RevokeCallbackToken()
    {
        var target = CreateTarget();
        target.AssignToken(new CallbackToken("tok", _now.AddHours(1), null));

        var result = target.RevokeCallbackToken(_now);

        result.IsSuccess.Should().BeTrue();
        target.CallbackToken!.RevokedAtUtc.Should().Be(_now);
    }

    [Fact]
    public void SetupTarget_Should_Fail_RevokeCallbackToken_When_None_Assigned()
    {
        var target = CreateTarget();

        var result = target.RevokeCallbackToken(_now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("No callback token");
    }
}

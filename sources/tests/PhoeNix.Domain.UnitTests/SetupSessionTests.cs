using System.Net;
using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.UnitTests;

public class SetupSessionTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private readonly SetupSessionId _sessionId = new(Guid.NewGuid());
    private readonly MachineId _machineId = new(Guid.NewGuid());
    private readonly SystemId _systemId = new(Guid.NewGuid());
    private readonly ConfigurationId _configId = new(Guid.NewGuid());
    private readonly DateTime _now = DateTime.UtcNow;

    private SetupSession CreateSession() =>
        SetupSession.Create(_sessionId, OwnerId, _now).Value;

    private SetupSession CreateSessionWithMachine()
    {
        var session = CreateSession();
        session.EnrollMachine(_machineId, _systemId, _configId, _now);
        return session;
    }

    [Fact]
    public void SetupSession_Should_Create_Successfully()
    {
        var result = SetupSession.Create(_sessionId, OwnerId, _now);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_sessionId);
        result.Value.StartTime.Should().Be(_now);
        result.Value.Targets.Should().BeEmpty();
        result.Value.SshCredential.Should().BeNull();
        result.Value.BootArtefactDescriptor.Should().BeNull();
    }

    [Fact]
    public void SetupSession_Should_AssignSshCredential()
    {
        var session = CreateSession();
        var credential = new SshCredential("pubkey", "certkey", _now.AddHours(1), null);

        var result = session.AssignSshCredential(credential, _now);

        result.IsSuccess.Should().BeTrue();
        session.SshCredential.Should().NotBeNull();
    }

    [Fact]
    public void SetupSession_Should_Fail_AssignSshCredential_When_Expired()
    {
        var session = CreateSession();
        var expired = new SshCredential("pubkey", "certkey", _now.AddHours(-1), null);

        var result = session.AssignSshCredential(expired, _now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("expired or revoked");
    }

    [Fact]
    public void SetupSession_Should_RevokeSshCredential()
    {
        var session = CreateSession();
        session.AssignSshCredential(new SshCredential("pk", "cert", _now.AddHours(1), null), _now);

        var result = session.RevokeSshCredential(_now);

        result.IsSuccess.Should().BeTrue();
        session.SshCredential!.RevokedAtUtc.Should().Be(_now);
    }

    [Fact]
    public void SetupSession_Should_Fail_RevokeSshCredential_When_None_Assigned()
    {
        var session = CreateSession();

        var result = session.RevokeSshCredential(_now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("No SSH credential");
    }

    [Fact]
    public void SetupSession_Should_Fail_RevokeSshCredential_When_Already_Revoked()
    {
        var session = CreateSession();
        var revoked = new SshCredential("pk", "cert", _now.AddHours(1), _now.AddMinutes(-5));
        session.AssignSshCredential(new SshCredential("pk", "cert", _now.AddHours(1), null), _now);
        session.RevokeSshCredential(_now);

        var result = session.RevokeSshCredential(_now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already revoked");
    }

    [Fact]
    public void SetupSession_Should_AssignBootstrapArtefact()
    {
        var session = CreateSession();

        var result = session.AssignBootstrapArtefact(
            "/nix/store/abc-kernel",
            "/nix/store/def-initrd",
            "init=/init");

        result.IsSuccess.Should().BeTrue();
        session.BootArtefactDescriptor.Should().NotBeNull();
        session.BootArtefactDescriptor!.Kernel.Should().Be("/nix/store/abc-kernel");
        session.BootArtefactDescriptor.RamDisk.Should().Be("/nix/store/def-initrd");
        session.BootArtefactDescriptor.Init.Should().Be("init=/init");
    }

    [Fact]
    public void SetupSession_Should_Fail_AssignBootstrapArtefact_When_Kernel_Not_Absolute()
    {
        var session = CreateSession();

        var result = session.AssignBootstrapArtefact("relative/kernel", "/nix/store/initrd", "");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Kernel location has to be absolute path");
    }

    [Fact]
    public void SetupSession_Should_Fail_AssignBootstrapArtefact_When_Initrd_Not_Absolute()
    {
        var session = CreateSession();

        var result = session.AssignBootstrapArtefact("/nix/store/kernel", "relative/initrd", "");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Ramdisk location has to be absolute path");
    }

    [Fact]
    public void SetupSession_Should_Fail_AssignBootstrapArtefact_When_Kernel_Not_Nix_Store()
    {
        var session = CreateSession();

        var result = session.AssignBootstrapArtefact("/usr/bin/kernel", "/nix/store/initrd", "");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Kernel location has to be a store path");
    }

    [Fact]
    public void SetupSession_Should_Fail_AssignBootstrapArtefact_When_Initrd_Not_Nix_Store()
    {
        var session = CreateSession();

        var result = session.AssignBootstrapArtefact("/nix/store/kernel", "/usr/bin/initrd", "");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Ramdisk location has to be a store path");
    }

    [Fact]
    public void SetupSession_Should_EnrollMachine()
    {
        var session = CreateSession();

        var result = session.EnrollMachine(_machineId, _systemId, _configId, _now);

        result.IsSuccess.Should().BeTrue();
        session.Targets.Should().ContainSingle(t => t.MachineId == _machineId);
    }

    [Fact]
    public void SetupSession_Should_Fail_EnrollMachine_When_Already_Enrolled()
    {
        var session = CreateSessionWithMachine();

        var result = session.EnrollMachine(_machineId, _systemId, _configId, _now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already enrolled");
        session.Targets.Should().ContainSingle();
    }

    [Fact]
    public void SetupSession_Should_UpdateMachineStage()
    {
        var session = CreateSessionWithMachine();

        var result = session.UpdateMachineStage(_machineId, SetupStage.WaitingForPxe, _now);

        result.IsSuccess.Should().BeTrue();
        session.Targets.Single().Stage.Should().Be(SetupStage.WaitingForPxe);
    }

    [Fact]
    public void SetupSession_Should_Fail_UpdateMachineStage_When_Not_Enrolled()
    {
        var session = CreateSession();

        var result = session.UpdateMachineStage(_machineId, SetupStage.WaitingForPxe, _now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("not enrolled");
    }

    [Fact]
    public void SetupSession_Should_RecordMachineFailure()
    {
        var session = CreateSessionWithMachine();
        var error = new Error("Test.Error", "Something went wrong");

        var result = session.RecordMachineFailure(_machineId, error, "TestSource", _now);

        result.IsSuccess.Should().BeTrue();
        var target = session.Targets.Single();
        target.LastErrorCode.Should().Be("Test.Error");
        target.LastErrorDescription.Should().Be("Something went wrong");
        target.LastErrorSource.Should().Be("TestSource");
    }

    [Fact]
    public void SetupSession_Should_Fail_RecordMachineFailure_When_Not_Enrolled()
    {
        var session = CreateSession();

        var result = session.RecordMachineFailure(_machineId, new Error("E", "msg"), "src", _now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("not enrolled");
    }

    [Fact]
    public void SetupSession_Should_ClearMachineFailure()
    {
        var session = CreateSessionWithMachine();
        session.RecordMachineFailure(_machineId, new Error("E", "msg"), "src", _now);

        var result = session.ClearMachineFailure(_machineId);

        result.IsSuccess.Should().BeTrue();
        session.Targets.Single().LastErrorCode.Should().BeNull();
    }

    [Fact]
    public void SetupSession_Should_AssignAndRevokeMachineCallbackToken()
    {
        var session = CreateSessionWithMachine();
        var token = new CallbackToken("tok123", _now.AddHours(1), null);

        session.AssignMachineCallbackToken(_machineId, token).IsSuccess.Should().BeTrue();
        session.Targets.Single().CallbackToken.Should().NotBeNull();

        session.RevokeMachineCallbackToken(_machineId, _now).IsSuccess.Should().BeTrue();
        session.Targets.Single().CallbackToken!.RevokedAtUtc.Should().Be(_now);
    }

    [Fact]
    public void SetupSession_Should_Fail_RevokeMachineCallbackToken_When_Already_Revoked()
    {
        var session = CreateSessionWithMachine();
        var token = new CallbackToken("tok123", _now.AddHours(1), null);
        session.AssignMachineCallbackToken(_machineId, token);
        session.RevokeMachineCallbackToken(_machineId, _now);

        var result = session.RevokeMachineCallbackToken(_machineId, _now);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already revoked");
    }

    [Fact]
    public void SetupSession_Should_ClearCallbackToken()
    {
        var session = CreateSessionWithMachine();
        session.AssignMachineCallbackToken(_machineId, new CallbackToken("tok", _now.AddHours(1), null));

        var result = session.ClearCallbackToken(_machineId);

        result.IsSuccess.Should().BeTrue();
        session.Targets.Single().CallbackToken.Should().BeNull();
    }

    [Fact]
    public void SetupSession_Should_RecordMachineIpAddress()
    {
        var session = CreateSessionWithMachine();
        var ip = IPAddress.Parse("10.0.0.1");

        var result = session.RecordMachineIpAddress(_machineId, ip);

        result.IsSuccess.Should().BeTrue();
        session.Targets.Single().IpAddress.Should().Be(ip);
    }

    [Fact]
    public void SetupSession_Should_AssignAndClearRankedDisks()
    {
        var session = CreateSessionWithMachine();
        var paths = new[] { "/dev/disk/by-id/disk1", "/dev/disk/by-id/disk2" };

        var assignResult = session.AssignRankedDisks(_machineId, paths);
        assignResult.IsSuccess.Should().BeTrue();
        session.Targets.Single().RankedDiskAssignments.Should().HaveCount(2);

        var clearResult = session.ClearRankedDisks(_machineId);
        clearResult.IsSuccess.Should().BeTrue();
        session.Targets.Single().RankedDiskAssignments.Should().BeEmpty();
    }

    [Fact]
    public void SetupSession_Should_Fail_Machine_Operations_When_Not_Enrolled()
    {
        var session = CreateSession();
        var notEnrolled = new MachineId(Guid.NewGuid());

        session.ClearMachineFailure(notEnrolled).IsFailure.Should().BeTrue();
        session.AssignMachineCallbackToken(notEnrolled, new CallbackToken("t", _now.AddHours(1), null)).IsFailure.Should().BeTrue();
        session.ClearCallbackToken(notEnrolled).IsFailure.Should().BeTrue();
        session.RevokeMachineCallbackToken(notEnrolled, _now).IsFailure.Should().BeTrue();
        session.RecordMachineIpAddress(notEnrolled, IPAddress.Loopback).IsFailure.Should().BeTrue();
        session.AssignRankedDisks(notEnrolled, new[] { "/dev/disk/by-id/x" }).IsFailure.Should().BeTrue();
        session.ClearRankedDisks(notEnrolled).IsFailure.Should().BeTrue();
    }
}

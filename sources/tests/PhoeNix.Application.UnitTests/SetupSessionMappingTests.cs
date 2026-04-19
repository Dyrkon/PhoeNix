using System.Net;
using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class SetupSessionMappingTests
{
    private readonly SetupSessionId _sessionId = new(Guid.NewGuid());
    private readonly MachineId _machineId = new(Guid.NewGuid());
    private readonly ConfigurationId _configId = new(Guid.NewGuid());
    private readonly SystemId _systemId = new(Guid.NewGuid());
    private readonly DateTime _now = DateTime.UtcNow;

    private SetupSession CreateSession()
    {
        return SetupSession.Create(_sessionId, _now).Value;
    }

    [Fact]
    public void MapSetupSessionToListDto_Should_Map_Empty_Session()
    {
        var session = CreateSession();

        var dto = SetupSessionMappings.MapSetupSessionToListDto(session);

        dto.SessionId.Should().Be(_sessionId.Value);
        dto.StartTime.Should().Be(_now);
        dto.TargetsTotal.Should().Be(0);
        dto.TargetsDone.Should().Be(0);
        dto.TargetsFailed.Should().Be(0);
        dto.LastTransitionTime.Should().BeNull();
    }

    [Fact]
    public void MapSetupSessionToListDto_Should_Count_Done_And_Failed_Targets()
    {
        var session = CreateSession();
        var machineId2 = new MachineId(Guid.NewGuid());
        var configId2 = new ConfigurationId(Guid.NewGuid());
        var systemId2 = new SystemId(Guid.NewGuid());

        session.EnrollMachine(_machineId, _systemId, _configId, _now);
        session.EnrollMachine(machineId2, systemId2, configId2, _now);
        session.UpdateMachineStage(_machineId, SetupStage.Finished, _now);
        session.UpdateMachineStage(machineId2, SetupStage.Failed, _now);

        var dto = SetupSessionMappings.MapSetupSessionToListDto(session);

        dto.TargetsTotal.Should().Be(2);
        dto.TargetsDone.Should().Be(1);
        dto.TargetsFailed.Should().Be(1);
        dto.LastTransitionTime.Should().NotBeNull();
    }

    [Fact]
    public void MapSetupSessionToListDto_Should_Also_Count_Cancelled_As_Done()
    {
        var session = CreateSession();
        session.EnrollMachine(_machineId, _systemId, _configId, _now);
        session.UpdateMachineStage(_machineId, SetupStage.Cancelled, _now);

        var dto = SetupSessionMappings.MapSetupSessionToListDto(session);

        dto.TargetsDone.Should().Be(1);
    }

    [Fact]
    public void MapSetupSessionToDto_Should_Map_Session_With_No_Targets()
    {
        var session = CreateSession();

        var dto = SetupSessionMappings.MapSetupSessionToDto(session, new List<Configuration>());

        dto.SessionId.Should().Be(_sessionId.Value);
        dto.StartTime.Should().Be(_now);
        dto.Targets.Should().BeEmpty();
        dto.CredentialsExpireAt.Should().BeNull();
    }

    [Fact]
    public void MapSetupSessionToDto_Should_Map_Enrolled_Target()
    {
        var session = CreateSession();
        session.EnrollMachine(_machineId, _systemId, _configId, _now);

        var dto = SetupSessionMappings.MapSetupSessionToDto(session, new List<Configuration>());

        dto.Targets.Should().ContainSingle();
        var target = dto.Targets.Single();
        target.MachineId.Should().Be(_machineId.Value);
        target.SetupStage.Should().Be(SetupStage.Created);
    }

    [Fact]
    public void MapSetupSessionToDto_Should_Resolve_System_Name_From_Configuration()
    {
        var session = CreateSession();
        session.EnrollMachine(_machineId, _systemId, _configId, _now);

        var config = Configuration.Create(_configId, "My Config", "Desc").Value;
        config.AddSystem(_systemId, Architecture.X86Linux, "web-server");

        var dto = SetupSessionMappings.MapSetupSessionToDto(session, new List<Configuration> { config });

        var target = dto.Targets.Single();
        target.SelectedConfigurationId.Should().Be(_configId.Value);
        target.SelectedConfiguration.Should().Be("My Config");
        target.SelectedSystemId.Should().Be(_systemId.Value);
        target.SelectedSystem.Should().Be("web-server");
    }

    [Fact]
    public void MapSetupSessionToDto_Should_Include_Ssh_Credential_Expiry()
    {
        var session = CreateSession();
        var expiresAt = _now.AddHours(1);
        session.AssignSshCredential(new SshCredential("pubkey", "certkey", expiresAt, null), _now);

        var dto = SetupSessionMappings.MapSetupSessionToDto(session, new List<Configuration>());

        dto.CredentialsExpireAt.Should().Be(expiresAt);
    }
}

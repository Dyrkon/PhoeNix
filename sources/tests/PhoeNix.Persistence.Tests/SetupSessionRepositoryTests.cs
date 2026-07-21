using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class SetupSessionRepositoryTests : PersistenceTestsBase
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());
    private ISetupSessionRepository SetupSessionRepository =>
        ServiceProvider.GetRequiredService<ISetupSessionRepository>();

    public SetupSessionRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    private static SetupSession CreateSession(DateTime startTime)
    {
        return SetupSession.Create(new SetupSessionId(Guid.NewGuid()), OwnerId, startTime).Value;
    }

    private static SetupSession CreateSessionWithActiveSsh(DateTime startTime)
    {
        var session = SetupSession.Create(new SetupSessionId(Guid.NewGuid()), OwnerId, startTime).Value;
        session.AssignSshCredential(new SshCredential("pk", "cert", startTime.AddHours(2), null), startTime);
        return session;
    }

    [Fact]
    public async Task GetWithEnrolledMachineAsync_Should_Return_Session_Containing_Machine()
    {
        var machineId = new MachineId(Guid.NewGuid());
        var configId = new ConfigurationId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var session = CreateSession(DateTime.UtcNow);
        session.EnrollMachine(machineId, systemId, configId, DateTime.UtcNow);

        await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await SetupSessionRepository.GetWithEnrolledMachineAsync(machineId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(session.Id);
    }

    [Fact]
    public async Task GetWithEnrolledMachineAsync_Should_Return_Null_When_Machine_Not_Enrolled()
    {
        var unknownMachineId = new MachineId(Guid.NewGuid());

        var result = await SetupSessionRepository.GetWithEnrolledMachineAsync(unknownMachineId, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetWithEnrolledMachineAsync_Should_Return_Most_Recent_Session()
    {
        var machineId = new MachineId(Guid.NewGuid());
        var configId = new ConfigurationId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var now = DateTime.UtcNow;

        var olderSession = CreateSession(now.AddHours(-2));
        olderSession.EnrollMachine(machineId, systemId, configId, now.AddHours(-2));

        var newerSession = CreateSession(now);
        newerSession.EnrollMachine(machineId, systemId, configId, now);

        await PhoeNixDbContextSUT.Set<SetupSession>().AddRangeAsync(olderSession, newerSession);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await SetupSessionRepository.GetWithEnrolledMachineAsync(machineId, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(newerSession.Id);
    }

    [Fact]
    public async Task GetPageAsync_Should_Return_Paged_Results()
    {
        var now = DateTime.UtcNow;
        for (var i = 0; i < 5; i++)
        {
            var session = CreateSession(now.AddMinutes(i));
            await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        }

        await PhoeNixDbContextSUT.SaveChangesAsync();

        var request = new SetupSessionsRequest(1, 3);
        var result = await SetupSessionRepository.GetSetupSessions(request, OwnerId, CancellationToken.None);

        result.TotalItems.Should().Be(5);
        result.Items.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetSetupSessions_Should_Return_Second_Page()
    {
        var now = DateTime.UtcNow;
        for (var i = 0; i < 5; i++)
        {
            var session = CreateSession(now.AddMinutes(i));
            await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        }

        await PhoeNixDbContextSUT.SaveChangesAsync();

        var request = new SetupSessionsRequest(2, 3);
        var result = await SetupSessionRepository.GetSetupSessions(request, OwnerId, CancellationToken.None);

        result.TotalItems.Should().Be(5);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSetupSessions_Should_Sort_Ascending_By_StartTime()
    {
        var now = DateTime.UtcNow;
        var times = new[] { now.AddHours(2), now, now.AddHours(1) };
        foreach (var time in times) await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(CreateSession(time));
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var request = new SetupSessionsRequest(SortDirection: SortDirection.Ascending);
        var result = await SetupSessionRepository.GetSetupSessions(request, OwnerId, CancellationToken.None);

        result.Items.Should().BeInAscendingOrder(s => s.StartTime);
    }

    [Fact]
    public async Task GetSetupSessions_Should_Sort_Descending_By_StartTime()
    {
        var now = DateTime.UtcNow;
        var times = new[] { now.AddHours(2), now, now.AddHours(1) };
        foreach (var time in times) await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(CreateSession(time));
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var request = new SetupSessionsRequest(SortDirection: SortDirection.Descending);
        var result = await SetupSessionRepository.GetSetupSessions(request, OwnerId, CancellationToken.None);

        result.Items.Should().BeInDescendingOrder(s => s.StartTime);
    }

    [Fact]
    public async Task HasActiveSessionAsync_Returns_False_When_No_Sessions()
    {
        var result = await SetupSessionRepository.HasActiveSessionAsync(OwnerId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(SetupStage.Created)]
    [InlineData(SetupStage.WaitingForPxe)]
    [InlineData(SetupStage.ArtefactsAssigned)]
    [InlineData(SetupStage.Bootstrapped)]
    [InlineData(SetupStage.Probed)]
    [InlineData(SetupStage.Orchestrated)]
    public async Task HasActiveSessionAsync_Returns_True_When_Session_Has_Non_Terminal_Target(SetupStage stage)
    {
        var now = DateTime.UtcNow;
        var session = CreateSession(now);
        var machineId = new MachineId(Guid.NewGuid());
        session.EnrollMachine(machineId, new SystemId(Guid.NewGuid()), new ConfigurationId(Guid.NewGuid()), now);
        if (stage != SetupStage.Created)
            session.UpdateMachineStage(machineId, stage, now);

        await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await SetupSessionRepository.HasActiveSessionAsync(OwnerId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(SetupStage.Finished)]
    [InlineData(SetupStage.Failed)]
    [InlineData(SetupStage.Cancelled)]
    public async Task HasActiveSessionAsync_Returns_False_When_All_Targets_Are_Terminal(SetupStage stage)
    {
        var now = DateTime.UtcNow;
        var session = CreateSession(now);
        var machineId = new MachineId(Guid.NewGuid());
        session.EnrollMachine(machineId, new SystemId(Guid.NewGuid()), new ConfigurationId(Guid.NewGuid()), now);
        session.UpdateMachineStage(machineId, stage, now);

        await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await SetupSessionRepository.HasActiveSessionAsync(OwnerId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveSessionAsync_Returns_True_When_No_Targets_And_Ssh_Active()
    {
        var now = DateTime.UtcNow;
        var session = CreateSessionWithActiveSsh(now);

        await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await SetupSessionRepository.HasActiveSessionAsync(OwnerId, CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasActiveSessionAsync_Returns_False_When_No_Targets_And_Ssh_Revoked()
    {
        var now = DateTime.UtcNow;
        var session = CreateSessionWithActiveSsh(now);
        session.RevokeSshCredential(now);

        await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await SetupSessionRepository.HasActiveSessionAsync(OwnerId, CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveSessionAsync_Returns_False_For_Different_Owner()
    {
        var now = DateTime.UtcNow;
        var otherId = new UserId(Guid.NewGuid());
        var session = SetupSession.Create(new SetupSessionId(Guid.NewGuid()), otherId, now).Value;
        session.AssignSshCredential(new SshCredential("pk", "cert", now.AddHours(2), null), now);

        await PhoeNixDbContextSUT.Set<SetupSession>().AddAsync(session);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await SetupSessionRepository.HasActiveSessionAsync(OwnerId, CancellationToken.None);

        result.Should().BeFalse();
    }
}
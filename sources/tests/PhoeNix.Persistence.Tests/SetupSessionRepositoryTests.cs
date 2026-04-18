using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PhoeNix.Application.Models.Setup;
using PhoeNix.Application.Repositories;
using PhoeNix.Common.Models;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Machines;
using PhoeNix.Domain.Entities.SetupSessions;
using PhoeNix.Domain.Entities.Systems;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class SetupSessionRepositoryTests : PersistenceTestsBase
{
    private ISetupSessionRepository SetupSessionRepository =>
        ServiceProvider.GetRequiredService<ISetupSessionRepository>();

    public SetupSessionRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    private static SetupSession CreateSession(DateTime startTime)
    {
        return SetupSession.Create(new SetupSessionId(Guid.NewGuid()), startTime).Value;
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
        var result = await SetupSessionRepository.GetSetupSessions(request, CancellationToken.None);

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
        var result = await SetupSessionRepository.GetSetupSessions(request, CancellationToken.None);

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
        var result = await SetupSessionRepository.GetSetupSessions(request, CancellationToken.None);

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
        var result = await SetupSessionRepository.GetSetupSessions(request, CancellationToken.None);

        result.Items.Should().BeInDescendingOrder(s => s.StartTime);
    }
}
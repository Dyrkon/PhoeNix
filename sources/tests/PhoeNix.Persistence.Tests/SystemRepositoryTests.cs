using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class SystemRepositoryWithIncludesTests : PersistenceTestsBase
{
    public SystemRepositoryWithIncludesTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task GetByIdAsync_ShouldLoadSystemWithModulesAndModuleDetails()
    {
        // Arrange
        var systemId = new SystemId(Guid.NewGuid());
        var systemResult = Domain.Entities.Systems.System.Create(systemId, Architecture.Aarch64Linux, "SystemX64");
        systemResult.IsSuccess.Should().BeTrue();
        var system = systemResult.Value;

        var moduleId = new ModuleId(Guid.NewGuid());
        var moduleResult = Module.Create(moduleId, "ModuleX", true, "",ModuleType.System, [Architecture.Aarch64Linux]);
        moduleResult.IsSuccess.Should().BeTrue();
        var module = moduleResult.Value;

        PhoeNixDbContextSUT.Add(module);
        PhoeNixDbContextSUT.Add(system);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var addResult = system.AddModule(module);
        addResult.IsSuccess.Should().BeTrue();
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var fetched = await SystemRepository.GetByIdAsync(systemId, CancellationToken.None);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.Modules.Should().ContainSingle();

        var sysModule = fetched.Modules.First();
        sysModule.ModuleId.Should().Be(moduleId);
        sysModule.Module.Should().NotBeNull();
        sysModule.Module!.Name.Should().Be("ModuleX");
        sysModule.Module.Type.Should().Be(ModuleType.System);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldLoadSystemWithModulesAndModuleDetails()
    {
        // Arrange
        var systemId = new SystemId(Guid.NewGuid());
        var systemResult = Domain.Entities.Systems.System.Create(systemId, Architecture.Aarch64Linux, "NamedSystem");
        systemResult.IsSuccess.Should().BeTrue();
        var system = systemResult.Value;

        var moduleId = new ModuleId(Guid.NewGuid());
        var moduleResult = Module.Create(moduleId, "NestedModule", false, "",ModuleType.System, [Architecture.Aarch64Linux]);
        moduleResult.IsSuccess.Should().BeTrue();
        var module = moduleResult.Value;

        PhoeNixDbContextSUT.Add(module);
        PhoeNixDbContextSUT.Add(system);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var addResult = system.AddModule(module);
        addResult.IsSuccess.Should().BeTrue();
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var fetched = await SystemRepository.GetByNameAsync("Named", CancellationToken.None);

        // Assert
        fetched.Should().NotBeNull();
        fetched!.Name.Should().Be("NamedSystem");
        fetched.Modules.Should().ContainSingle();

        var sysModule = fetched.Modules.First();
        sysModule.ModuleId.Should().Be(moduleId);
        sysModule.Module.Should().NotBeNull();
        sysModule.Module!.Name.Should().Be("NestedModule");
        sysModule.Module.Enabled.Should().BeFalse();
    }
}

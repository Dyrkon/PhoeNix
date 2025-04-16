using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class SystemMappingsTests
{
    [Fact]
    public void MapSystemToListDto_Should_Map_Correctly()
    {
        var systemId = new SystemId(Guid.NewGuid());
        var system = Domain.Entities.Systems.System.Create(systemId, Architecture.X86Linux, "My System").Value;

        var result = SystemMappings.MapSystemToListDto(system);

        result.Should().NotBeNull();
        result.Id.Should().Be(system.Id);
        result.Name.Should().Be("My System");
        result.Architecture.Should().Be(Architecture.X86Linux);
    }

    [Fact]
    public void MapSystemToDto_Should_Map_Full_System_With_Modules()
    {
        var systemId = new SystemId(Guid.NewGuid());
        var moduleId = new ModuleId(Guid.NewGuid());

        var system = Domain.Entities.Systems.System.Create(systemId, Architecture.Aarch64Linux, "System A").Value;
        var module = Module.Create(moduleId, "My Module", true, ModuleType.System,
            [Architecture.Aarch64Linux]).Value;

        var sysModule = SystemModule.Create(new SystemModuleId(Guid.NewGuid()), system.Id, moduleId).Value;
        sysModule.SetModule(module);

        typeof(Domain.Entities.Systems.System)
            .GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(system, new List<SystemModule> { sysModule });

        var result = SystemMappings.MapSystemToDto(system);

        result.Should().NotBeNull();
        result.Id.Should().Be(system.Id);
        result.Name.Should().Be(system.Name);
        result.Architecture.Should().Be(system.Architecture);
        result.Modules.Should().ContainSingle(m => m.Id == module.Id && m.Name == module.Name);
    }
}
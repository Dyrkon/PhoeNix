using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class SystemMappingsTests
{
    [Fact]
    public void MapSystemToListDto_Should_Map_Correctly()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var system = Domain.Entities.Systems.System.Create(systemId, configId, Architecture.X86Linux, "Sys").Value;

        var dto = SystemMappings.MapSystemToListDto(system);

        dto.Id.Should().Be(systemId.Value);
        dto.Architecture.Should().Be(Architecture.X86Linux);
        dto.Name.Should().Be("Sys");
    }

    [Fact]
    public void MapSystemToDto_Should_Include_Modules()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());
        var system = Domain.Entities.Systems.System.Create(systemId, configId, Architecture.X86Linux, "Sys").Value;

        var mtid = new ModuleTemplateId(Guid.NewGuid());
        system.AddModule(mtid, new List<Architecture> { Architecture.X86Linux }, true).IsSuccess.Should().BeTrue();

        var dto = SystemMappings.MapSystemToDto(system);

        dto.Id.Should().Be(systemId.Value);
        dto.Architecture.Should().Be(Architecture.X86Linux);
        dto.Name.Should().Be("Sys");
        dto.Modules.Should().ContainSingle(m => m.Id == system.Modules.Single().Id.Value);
    }
}

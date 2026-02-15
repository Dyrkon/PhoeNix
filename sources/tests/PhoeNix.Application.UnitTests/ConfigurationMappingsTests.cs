using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class ConfigurationMappingsTests
{
    [Fact]
    public void MapConfigurationToListDto_Should_Map_Correctly()
    {
        var id = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(id, "Config A", "Description").Value;

        var dto = ConfigurationMappings.MapConfigurationToListDto(config);

        dto.Id.Should().Be(config.Id);
        dto.Title.Should().Be("Config A");
        dto.Description.Should().Be("Description");
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_Full_Configuration()
    {
        var id = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(id, "Full Config", "Detailed description").Value;

        // Input (new signature requires configurationId)
        var inputResult = config.AddInput("github:nixos", "nixpkgs");
        inputResult.IsSuccess.Should().BeTrue();

        // ModuleValue list is created through Configuration.AddModule(moduleTemplateId, enabled)
        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var addModule = config.AddModule(moduleTemplateId, true);
        addModule.IsSuccess.Should().BeTrue();

        // System list is created through Configuration.AddSystem(systemId, architecture, name)
        var systemId = new Domain.Entities.Systems.SystemId(Guid.NewGuid());
        var addSystem = config.AddSystem(systemId, Architecture.X86Linux, "Name");
        addSystem.IsSuccess.Should().BeTrue();

        var dto = ConfigurationMappings.MapConfigurationToDto(config);

        dto.Id.Should().Be(config.Id);
        dto.Title.Should().Be("Full Config");
        dto.Description.Should().Be("Detailed description");

        dto.Inputs.Should().ContainSingle(i => i.Name == "nixpkgs" && i.Source == "github:nixos");
        dto.Modules.Should().ContainSingle(m => m.ModuleTemplateId == moduleTemplateId);
        dto.Systems.Should().ContainSingle(s => s.Id == systemId && s.Architecture == Architecture.X86Linux);

        // SupportedArchitectures comes from SupportedSystemArchitectures()
        dto.SupportedArchitectures.Should().ContainSingle().And.Contain(Architecture.X86Linux);
    }
}
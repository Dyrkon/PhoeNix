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

        config.AddInput("github:nixos", "nixpkgs").IsSuccess.Should().BeTrue();

        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        config.AddModule(moduleTemplateId, true).IsSuccess.Should().BeTrue();

        var systemId = new Domain.Entities.Systems.SystemId(Guid.NewGuid());
        config.AddSystem(systemId, Architecture.X86Linux, "Name").IsSuccess.Should().BeTrue();

        var dto = ConfigurationMappings.MapConfigurationToDto(config);

        dto.Id.Should().Be(config.Id);
        dto.Title.Should().Be("Full Config");
        dto.Description.Should().Be("Detailed description");

        dto.Inputs.Should().ContainSingle(i => i.Name == "nixpkgs" && i.Source == "github:nixos");

        dto.Modules.Should().ContainSingle();
        dto.Modules.Single().Enabled.Should().BeTrue();
        dto.Modules.Single().EditableValues.Should().BeEmpty();

        dto.Systems.Should().ContainSingle(s => s.Id == systemId && s.Architecture == Architecture.X86Linux);

        dto.SupportedArchitectures.Should().ContainSingle().And.Contain(Architecture.X86Linux);
    }
}
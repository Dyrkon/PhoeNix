using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class ConfigurationMappingsTests
{
    [Fact]
    public void MapFlakeToListDto_Should_Map_Correctly()
    {
        var id = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(id, "Config A", "Description").Value;

        var dto = ConfigurationMappings.MapConfigurationToListDto(config);

        dto.Id.Should().Be(config.TemplateId);
        dto.Title.Should().Be("Config A");
        dto.Description.Should().Be("Description");
    }

    [Fact]
    public void MapFlakeToDto_Should_Map_Full_Configuration()
    {
        var id = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(id, "Full Config", "Detailed description").Value;

        var module = ModuleTemplate.Create(new ModuleTemplateId(Guid.NewGuid()), "mod", true, ModuleType.System,
            [Architecture.X86Linux]).Value;
        var system = PhoeNix.Domain.Entities.Systems.System
            .Create(new SystemId(Guid.NewGuid()), Architecture.X86Linux, "Name").Value;
        var input = Input.Create(new InputId(Guid.NewGuid()), "github:nixos", "nixpkgs").Value;

        var userId = new UserId(Guid.NewGuid());

        config.AddInput(input.TemplateId);
        config.AddModule(module.TemplateId);
        config.AddSystem(system.TemplateId);

        var cm = ModuleValue
            .Create(new ModuleValueId(Guid.NewGuid()), config.TemplateId, module.TemplateId).Value;
        typeof(ModuleValue).GetProperty(nameof(ModuleValue.Module))!.SetValue(cm, module);

        var cs = ConfigurationSystem
            .Create(new ConfigurationSystemId(Guid.NewGuid()), config.TemplateId, system.TemplateId).Value;
        cs.SetSystem(system);

        var ci = ConfigurationInput
            .Create(new ConfigurationInputId(Guid.NewGuid()), config.TemplateId, input.TemplateId).Value;
        ci.SetInput(input);

        SetPrivateField(config, "_modules", [cm]);
        SetPrivateField(config, "_systems", [cs]);
        SetPrivateField(config, "_inputs", [ci]);

        var dto = ConfigurationMappings.MapConfigurationToDto(config);

        dto.Title.Should().Be("Full Config");
        dto.Inputs.Should().ContainSingle(i => i.Name == input.Name);
        dto.Modules.Should().ContainSingle(m => m.Name == module.Name);
        dto.Systems.Should().ContainSingle(s => s.Id == system.TemplateId);
        dto.SupportedArchitectures.Should().Contain(Architecture.X86Linux);
    }


    private static void SetPrivateField<T>(Configuration config, string fieldName, List<T> value)
    {
        typeof(Configuration)
            .GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(config, value);
    }
}
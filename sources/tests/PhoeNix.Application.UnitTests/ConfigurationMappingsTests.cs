using System.Reflection;
using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class ConfigurationMappingsTests
{
    private static readonly UserId OwnerId = new(Guid.NewGuid());

    [Fact]
    public void MapConfigurationToListDto_Should_Map_Correctly()
    {
        var id = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(id, OwnerId, "Config A", "Description").Value;

        var dto = ConfigurationMappings.MapConfigurationToListDto(config);

        dto.Id.Should().Be(config.Id.Value);
        dto.Title.Should().Be("Config A");
        dto.Description.Should().Be("Description");
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_Full_Configuration()
    {
        var id = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(id, OwnerId, "Full Config", "Detailed description").Value;

        config.AddInput("github:nixos", "nixpkgs").IsSuccess.Should().BeTrue();

        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        config.AddModule(moduleTemplateId, true).IsSuccess.Should().BeTrue();

        var systemId = new SystemId(Guid.NewGuid());
        config.AddSystem(systemId, Architecture.X86Linux, "Name").IsSuccess.Should().BeTrue();

        var moduleTemplate = ModuleTemplate.Create(moduleTemplateId, OwnerId, "MyModule", true, ModuleType.Generic,
            new List<Architecture> { Architecture.X86Linux }).Value;
        var templatesById = new Dictionary<ModuleTemplateId, ModuleTemplate> { { moduleTemplateId, moduleTemplate } };

        var dto = ConfigurationMappings.MapConfigurationToDto(config, templatesById);

        dto.Id.Should().Be(config.Id.Value);
        dto.Title.Should().Be("Full Config");
        dto.Description.Should().Be("Detailed description");

        dto.Inputs.Should().ContainSingle(i => i.Name == "nixpkgs" && i.Source == "github:nixos");

        dto.Modules.Should().ContainSingle();
        dto.Modules.Single().Enabled.Should().BeTrue();
        dto.Modules.Single().Entries.Should().BeEmpty();

        dto.Systems.Should().ContainSingle(s => s.Id == systemId.Value && s.Architecture == Architecture.X86Linux);

        dto.SupportedArchitectures.Should().ContainSingle().And.Contain(Architecture.X86Linux);
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_TextValue_Entry()
    {
        var (config, module, moduleTemplateId) = CreateConfigWithModule();

        InjectEditableValues(module, new List<EntryValue>
        {
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "hello", "TXT", "TXT").Value
        });

        var dto = MapConfig(config, moduleTemplateId);
        var entry = dto.Modules.Single().Entries.Single();
        entry.Kind.Should().Be(EntryValueKind.Text);
        entry.Value.Should().Be("hello");
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_IntegerRangeValue_Entry()
    {
        var (config, module, moduleTemplateId) = CreateConfigWithModule();

        InjectEditableValues(module, new List<EntryValue>
        {
            IntegerRangeValue.Create(new EntryValueId(Guid.NewGuid()), "INT", "INT", 0, 100, 50).Value
        });

        var dto = MapConfig(config, moduleTemplateId);
        var entry = dto.Modules.Single().Entries.Single();
        entry.Kind.Should().Be(EntryValueKind.IntegerRange);
        entry.IntegerUpperValue.Should().Be(50);
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_DecimalRangeValue_Entry()
    {
        var (config, module, moduleTemplateId) = CreateConfigWithModule();

        InjectEditableValues(module, new List<EntryValue>
        {
            DecimalRangeValue.Create(new EntryValueId(Guid.NewGuid()), "DEC", "DEC", 0m, 10m, 5m).Value
        });

        var dto = MapConfig(config, moduleTemplateId);
        var entry = dto.Modules.Single().Entries.Single();
        entry.Kind.Should().Be(EntryValueKind.DecimalRange);
        entry.DecimalUpperValue.Should().Be(5m);
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_SingleChoiceValue_Entry()
    {
        var (config, module, moduleTemplateId) = CreateConfigWithModule();

        InjectEditableValues(module, new List<EntryValue>
        {
            SingleChoiceValue.Create(new EntryValueId(Guid.NewGuid()), "SC", "SC",
                new List<string> { "a", "b" }, "a").Value
        });

        var dto = MapConfig(config, moduleTemplateId);
        var entry = dto.Modules.Single().Entries.Single();
        entry.Kind.Should().Be(EntryValueKind.SingleChoice);
        entry.Options.Should().Contain("a");
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_ListValue_Entry()
    {
        var (config, module, moduleTemplateId) = CreateConfigWithModule();

        InjectEditableValues(module, new List<EntryValue>
        {
            ListValue.Create(new EntryValueId(Guid.NewGuid()), "LV", "LV", new List<string> { "x" }).Value
        });

        var dto = MapConfig(config, moduleTemplateId);
        var entry = dto.Modules.Single().Entries.Single();
        entry.Kind.Should().Be(EntryValueKind.List);
        entry.ListItems.Should().Contain("x");
    }

    [Fact]
    public void MapConfigurationToDto_Should_Map_System_Module_Entries()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, OwnerId, "C", "D").Value;
        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        var systemId = new SystemId(Guid.NewGuid());

        config.AddSystem(systemId, Architecture.X86Linux, "S").IsSuccess.Should().BeTrue();
        config.AddSystemModule(systemId, moduleTemplateId, new List<Architecture> { Architecture.X86Linux }, true)
            .IsSuccess.Should().BeTrue();

        var systemModule = config.SystemSpecifications.Single().Modules.Single();
        InjectEditableValues(systemModule, new List<EntryValue>
        {
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "v", "SYS", "SYS").Value
        });

        var moduleTemplate = ModuleTemplate.Create(moduleTemplateId, OwnerId, "M", true, ModuleType.Generic,
            new List<Architecture> { Architecture.X86Linux }).Value;
        var templatesById = new Dictionary<ModuleTemplateId, ModuleTemplate> { { moduleTemplateId, moduleTemplate } };

        var dto = ConfigurationMappings.MapConfigurationToDto(config, templatesById);
        dto.Systems.Single().Modules.Single().Entries.Should().ContainSingle(e => e.Kind == EntryValueKind.Text);
    }

    private static (Configuration config, ModuleValue module, ModuleTemplateId moduleTemplateId)
        CreateConfigWithModule()
    {
        var configId = new ConfigurationId(Guid.NewGuid());
        var config = Configuration.Create(configId, OwnerId, "C", "D").Value;
        var moduleTemplateId = new ModuleTemplateId(Guid.NewGuid());
        config.AddModule(moduleTemplateId, true).IsSuccess.Should().BeTrue();
        var module = config.Modules.Single();
        return (config, module, moduleTemplateId);
    }

    private static Contracts.Configurations.ConfigurationResponse MapConfig(
        Configuration config, ModuleTemplateId moduleTemplateId)
    {
        var moduleTemplate = ModuleTemplate.Create(moduleTemplateId, OwnerId, "M", true, ModuleType.Generic,
            new List<Architecture> { Architecture.X86Linux }).Value;
        var templatesById = new Dictionary<ModuleTemplateId, ModuleTemplate> { { moduleTemplateId, moduleTemplate } };
        return ConfigurationMappings.MapConfigurationToDto(config, templatesById);
    }

    private static void InjectEditableValues(ModuleValue moduleValue, List<EntryValue> values)
    {
        var field = typeof(ModuleValue).GetField("_editableValues",
            BindingFlags.NonPublic | BindingFlags.Instance);
        field.Should().NotBeNull();
        field!.SetValue(moduleValue, values);
    }
}
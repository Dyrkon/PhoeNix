using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Modules;

namespace PhoeNix.Application.UnitTests;

public class ModuleMappingsTests
{
    [Fact]
    public void MapModuleToListDto_Should_Map_Correctly()
    {
        var id = new ModuleTemplateId(Guid.NewGuid());
        var module = ModuleTemplate
            .Create(id, "Test Module", true, ModuleType.Generic, new List<Architecture> { Architecture.Aarch64Linux })
            .Value;

        var dto = ModuleMappings.MapModuleToListDto(module);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Test Module");
        dto.Type.Should().Be(ModuleType.Generic);
    }

    [Fact]
    public void MapModuleToDto_Should_Map_All_Fields_Correctly()
    {
        var id = new ModuleTemplateId(Guid.NewGuid());
        var architecture = Architecture.Aarch64Linux;

        var module = ModuleTemplate
            .Create(id, "Init", false, ModuleType.Generic, new List<Architecture> { architecture })
            .Value;

        var def1 = new EntryValueDefinition(
            id,
            "VALUE_ONE",
            "{VALUE_ONE}",
            UserInputType.Text);

        var def2 = new EntryValueDefinition(
            id,
            "VALUE_TWO",
            "{VALUE_TWO}",
            UserInputType.Text);

        // ChangeContent requires every definition.Name to be present in content
        var content = "VALUE_ONE and VALUE_TWO";
        var change = module.ChangeContent(content, new List<EntryValueDefinition> { def1, def2 });
        change.IsSuccess.Should().BeTrue();

        var dto = ModuleMappings.MapModuleToDto(module);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Init");
        dto.Type.Should().Be(ModuleType.Generic);
        dto.Content.Should().Be(content);

        dto.SupportedArchitectures.Should().ContainSingle().Which.Should().Be(architecture);

        dto.EditableValueTypes.Should().HaveCount(2);
        dto.EditableValueTypes.Should().ContainEquivalentOf(
            new EntryValueDefinitionResponse("VALUE_ONE", "{VALUE_ONE}", UserInputType.Text));
        dto.EditableValueTypes.Should().ContainEquivalentOf(
            new EntryValueDefinitionResponse("VALUE_TWO", "{VALUE_TWO}", UserInputType.Text));
    }

    [Fact]
    public void MapEntryValueDefinitionToDto_Should_Map_All_Fields_Correctly()
    {
        var def = new EntryValueDefinition(
            new ModuleTemplateId(Guid.NewGuid()),
            "Entry1",
            "{Entry1}",
            UserInputType.Text);

        var dto = ModuleMappings.MapEntryValueDefinitionToDto(def);

        dto.Name.Should().Be("Entry1");
        dto.Placeholder.Should().Be("{Entry1}");
        dto.InputType.Should().Be(UserInputType.Text);
    }

    [Fact]
    public void MapEntryValueToDto_Should_Map_All_Fields_Correctly()
    {
        var id = new EntryValueId(Guid.NewGuid());
        var entry = TextValue.Create(id, "42", "Entry1", "ph1").Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Name.Should().Be("Entry1");
        dto.Placeholder.Should().Be("ph1");
        dto.Value.Should().Be("42");
    }
}
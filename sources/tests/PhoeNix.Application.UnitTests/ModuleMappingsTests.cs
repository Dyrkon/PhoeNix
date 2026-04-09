using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Application.Models.Modules;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class ModuleMappingsTests
{
    [Fact]
    public void MapModuleToListDto_Should_Map_Correctly()
    {
        var id = new ModuleTemplateId(Guid.NewGuid());
        var module = ModuleTemplate.Create(id, "Test Module", true, ModuleType.Generic,
            new List<Architecture> { Architecture.Aarch64Linux }).Value;

        var dto = ModuleMappings.MapModuleToListDto(module);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Test Module");
        dto.Type.Should().Be(ModuleType.Generic);
    }

    [Fact]
    public void MapModuleToDto_Should_Map_All_Fields_Correctly()
    {
        var id = new ModuleTemplateId(Guid.NewGuid());
        var module = ModuleTemplate.Create(id, "Init", true, ModuleType.Generic,
            new List<Architecture> { Architecture.Aarch64Linux }).Value;

        var def = new EntryValueDefinition(
            id,
            "VALUE_ONE",
            "VALUE_ONE",
            EntryBindingKind.UserProvided,
            EntryValueKind.Text);

        module.ChangeContent("VALUE_ONE", new List<EntryValueDefinition> { def }).IsSuccess.Should().BeTrue();

        var dto = ModuleMappings.MapModuleToDto(module);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Init");
        dto.Type.Should().Be(ModuleType.Generic);
        dto.Content.Should().Be("VALUE_ONE");
        dto.SupportedArchitectures.Should().ContainSingle().Which.Should().Be(Architecture.Aarch64Linux);

        dto.EditableValueTypes.Should().ContainSingle().Which.Should()
            .BeEquivalentTo(new EntryValueDefinitionResponse("VALUE_ONE", "VALUE_ONE",
                EntryBindingKind.UserProvided, EntryValueKind.Text,
                null, null, null, null, null, null, null, null, null));
    }

    [Fact]
    public void MapEntryValueToDto_Should_Map_All_Fields_Correctly()
    {
        var entry = TextValue.Create(new EntryValueId(Guid.NewGuid()), "42", "Entry1", "ph1").Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Name.Should().Be("Entry1");
        dto.Placeholder.Should().Be("ph1");
        dto.Value.Should().Be("42");
    }

    [Fact]
    public void MapModuleValueToDto_Should_Map_ModuleValue()
    {
        var moduleValue = ModuleValue
            .Create(new ModuleValueId(Guid.NewGuid()), new ModuleTemplateId(Guid.NewGuid()), true).Value;

        moduleValue.ChangeEntry(
            [TextValue.Create(new EntryValueId(Guid.NewGuid()), "hello", "E1", "{E1}").Value],
            null).IsSuccess.Should().BeTrue();

        var dto = ModuleMappings.MapModuleValueToDto(moduleValue);

        dto.Id.Should().Be(moduleValue.Id);
        dto.Enabled.Should().BeTrue();
        dto.EditableValues.Should().ContainSingle(e => e.Name == "E1" && e.Placeholder == "{E1}" && e.Value == "hello");
    }
}
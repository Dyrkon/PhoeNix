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

        dto.Id.Should().Be(id.Value);
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

        dto.Id.Should().Be(id.Value);
        dto.Name.Should().Be("Init");
        dto.Type.Should().Be(ModuleType.Generic);
        dto.Content.Should().Be("VALUE_ONE");
        dto.SupportedArchitectures.Should().ContainSingle().Which.Should().Be(Architecture.Aarch64Linux);

        dto.EditableValueTypes.Should().ContainSingle().Which.Should()
            .BeEquivalentTo(new EntryValueDefinitionResponse("VALUE_ONE", "VALUE_ONE",
                EntryBindingKind.UserProvided, EntryValueKind.Text,
                null, null, null, null, null, null, false, Array.Empty<string>(), null));
    }

    [Fact]
    public void MapEntryValueToDto_TextValue_Should_Map_Correctly()
    {
        var entry = TextValue.Create(new EntryValueId(Guid.NewGuid()), "42", "Entry1", "ph1").Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Name.Should().Be("Entry1");
        dto.Placeholder.Should().Be("ph1");
        dto.Value.Should().Be("42");
        dto.Kind.Should().Be(EntryValueKind.Text);
    }

    [Fact]
    public void MapEntryValueToDto_IntegerRangeValue_Should_Map_Correctly()
    {
        var entry = IntegerRangeValue.Create(
            new EntryValueId(Guid.NewGuid()), "RangeInt", "RNG", 0, 100, 75, 25).Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Kind.Should().Be(EntryValueKind.IntegerRange);
        dto.IntegerUpperValue.Should().Be(75);
        dto.IntegerLowerValue.Should().Be(25);
    }

    [Fact]
    public void MapEntryValueToDto_DecimalRangeValue_Should_Map_Correctly()
    {
        var entry = DecimalRangeValue.Create(
            new EntryValueId(Guid.NewGuid()), "RangeDec", "DRNG", 0m, 10m, 7.5m, 2.5m).Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Kind.Should().Be(EntryValueKind.DecimalRange);
        dto.DecimalUpperValue.Should().Be(7.5m);
        dto.DecimalLowerValue.Should().Be(2.5m);
    }

    [Fact]
    public void MapEntryValueToDto_SingleChoiceValue_Should_Map_Correctly()
    {
        var entry = SingleChoiceValue.Create(
            new EntryValueId(Guid.NewGuid()), "Choice", "CHC",
            new List<string> { "dark", "light" }, "dark").Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Kind.Should().Be(EntryValueKind.SingleChoice);
        dto.Value.Should().Be("dark");
        dto.Options.Should().Contain("dark").And.Contain("light");
    }

    [Fact]
    public void MapEntryValueToDto_ListValue_Should_Map_Correctly()
    {
        var entry = ListValue.Create(
            new EntryValueId(Guid.NewGuid()), "Items", "LST",
            new List<string> { "one", "two" }).Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Kind.Should().Be(EntryValueKind.List);
        dto.ListItems.Should().Contain("one").And.Contain("two");
    }

    [Fact]
    public void MapModuleValueToDto_Should_Map_ModuleValue()
    {
        var moduleValue = ModuleValue
            .Create(new ModuleValueId(Guid.NewGuid()), new ModuleTemplateId(Guid.NewGuid()), true).Value;

        moduleValue.ReplaceEntries(
            [TextValue.Create(new EntryValueId(Guid.NewGuid()), "hello", "E1", "E1").Value]
        ).IsSuccess.Should().BeTrue();

        var dto = ModuleMappings.MapModuleValueToDto(moduleValue);

        dto.Id.Should().Be(moduleValue.Id.Value);
        dto.Enabled.Should().BeTrue();
        dto.EditableValues.Should().ContainSingle(e => e.Name == "E1" && e.Value == "hello");
    }

    [Fact]
    public void MapModuleValueToListDto_Should_Map_Id_And_Enabled()
    {
        var moduleValue = ModuleValue
            .Create(new ModuleValueId(Guid.NewGuid()), new ModuleTemplateId(Guid.NewGuid()), false).Value;

        var dto = ModuleMappings.MapModuleValueToListDto(moduleValue);

        dto.Id.Should().Be(moduleValue.Id.Value);
        dto.Enabled.Should().BeFalse();
    }
}

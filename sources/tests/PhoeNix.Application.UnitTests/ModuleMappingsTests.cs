using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class ModuleMappingsTests
{
    [Fact]
    public void MapModuleToListDto_Should_Map_Correctly()
    {
        var moduleId = new ModuleId(Guid.NewGuid());

        var module = Module.Create(moduleId, "MyModule", true, ModuleType.Generic,
            [Architecture.X86Linux]).Value;

        var result = ModuleMappings.MapModuleToListDto(module);

        result.Should().NotBeNull();
        result.Id.Should().Be(module.Id);
        result.Name.Should().Be("MyModule");
        result.Enabled.Should().BeTrue();
        result.Type.Should().Be(ModuleType.Generic);
    }

    [Fact]
    public void MapModuleToDto_Should_Map_Full_Module()
    {
        var moduleId = new ModuleId(Guid.NewGuid());

        var module = Module.Create(moduleId, "MyModule", true, ModuleType.System,
            [Architecture.X86Linux, Architecture.Aarch64Linux]).Value;

        var entryId = new ModuleEntryId(Guid.NewGuid());
        var placeholder = Guid.NewGuid();

        var entry = ModuleEntry.Create(entryId).Value;

        var textValue = TextValue.Create(new EntryValueId(Guid.NewGuid()),"initial", "MyVar", placeholder).Value;
        entry.EditContent($"echo {placeholder}", [textValue]);

        module.AddEntry(entry);

        var result = ModuleMappings.MapModuleToDto(module);

        result.Should().NotBeNull();
        result.Id.Should().Be(module.Id);
        result.Name.Should().Be(module.Name);
        result.Enabled.Should().BeTrue();
        result.Type.Should().Be(ModuleType.System);
        result.ModuleEntries.Should().ContainSingle(e => e.Id == entry.Id);
        result.SupportedArchitectures.Should().BeEquivalentTo(module.SupportedArchitectures);
    }

    [Fact]
    public void MapModuleEntryToDto_Should_Map_Correctly()
    {
        var entryId = new ModuleEntryId(Guid.NewGuid());
        var placeholder = Guid.NewGuid();

        var entry = ModuleEntry.Create(entryId).Value;

        var textValue = TextValue.Create(new EntryValueId(Guid.NewGuid()),"initVal", "MyText", placeholder).Value;
        entry.EditContent($"#{placeholder}", [textValue]);

        var result = ModuleMappings.MapModuleEntryToDto(entry);

        result.Should().NotBeNull();
        result.Id.Should().Be(entry.Id);
        result.Content.Should().Be($"#{placeholder}");
        result.EntryValues.Should().ContainSingle(v => v.Placeholder == placeholder);
    }

    [Fact]
    public void MapEntryValueToDto_Should_Map_TextValue()
    {
        var placeholder = Guid.NewGuid();
        var value = TextValue.Create(new EntryValueId(Guid.NewGuid()),"init", "TextName", placeholder).Value;

        var result = ModuleMappings.MapEntryValueToDto(value);

        result.Should().NotBeNull();
        result.Name.Should().Be("TextName");
        result.Placeholder.Should().Be(placeholder);
        result.Value.Should().Be("init");
    }
}
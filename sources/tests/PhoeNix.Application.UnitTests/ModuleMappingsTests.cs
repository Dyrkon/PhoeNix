using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Models.Modules;
using Xunit.Abstractions;

namespace PhoeNix.Application.UnitTests;

public class ModuleMappingsTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ModuleMappingsTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void MapModuleToListDto_Should_Map_Correctly()
    {
        var id = new ModuleId(Guid.NewGuid());
        var module = ModuleTemplate.Create(id, "Test Module", true, ModuleType.Generic, [Architecture.Aarch64Linux])
            .Value;

        var dto = ModuleMappings.MapModuleToListDto(module);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Test Module");
        dto.Enabled.Should().BeTrue();
        dto.Type.Should().Be(ModuleType.Generic);
    }

    [Fact]
    public void MapModuleToDto_Should_Map_All_Fields_Correctly()
    {
        var id = new ModuleId(Guid.NewGuid());
        var entryId = new EntryValueId(Guid.NewGuid());
        var architecture = Architecture.Aarch64Linux;

        var entry = TextValue.Create(entryId, "Init", "Name", "Init").Value;
        var moduleResult = ModuleTemplate.Create(id, "Init", false, ModuleType.Generic,
            [architecture]);
        var module = moduleResult.Value;
        module.ChangeContent("Mod1 Name Placeholder1", [entry]);

        var dto = ModuleMappings.MapModuleToDto(module);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Init");
        dto.Enabled.Should().BeFalse();
        dto.Type.Should().Be(ModuleType.Generic);
        dto.Content.Should().Be("Mod1 Name Placeholder1");
        dto.SupportedArchitectures.Should().ContainSingle().Which.Should().Be(architecture);
        dto.EntryValues.Should().ContainSingle();
        dto.EntryValues[0].Should().BeEquivalentTo(new EntryValueResponse(entryId, "Name", "Init", "Init"));
    }

    [Fact]
    public void MapEntryValueToDto_Should_Map_All_Fields_Correctly()
    {
        var id = new EntryValueId(Guid.NewGuid());
        var entry = TextValue.Create(id, "42", "Entry1", "ph1").Value;

        var dto = ModuleMappings.MapEntryValueToDto(entry);

        dto.Id.Should().Be(id);
        dto.Name.Should().Be("Entry1");
        dto.Placeholder.Should().Be("ph1");
        dto.Value.Should().Be("42");
    }
}
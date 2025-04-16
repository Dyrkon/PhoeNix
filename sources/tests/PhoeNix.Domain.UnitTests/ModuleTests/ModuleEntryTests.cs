using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using Xunit.Abstractions;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class ModuleEntryTests
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ModuleEntryId _entryId = new(Guid.NewGuid());
    private readonly Guid _placeholder1 = Guid.NewGuid();
    private readonly Guid _placeholder2 = Guid.NewGuid();

    public ModuleEntryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void ModuleEntry_Should_Create_Empty_By_Default()
    {
        var result = ModuleEntry.Create(_entryId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().BeEmpty();
        result.Value.Editablevalues.Should().BeEmpty();
    }

    [Fact]
    public void ModuleEntry_Should_EditContent_When_All_Placeholders_Are_Present()
    {
        var textValue = TextValue.Create(new EntryValueId(Guid.NewGuid()), "init", "Title", _placeholder1).Value;
        var rangeValue = RangeValue<int>
            .Create(new EntryValueId(Guid.NewGuid()), "Range", _placeholder2, 100, 0, 80, 20).Value;

        var content = $"{{{_placeholder1}}}\nrange={{ {_placeholder2} }}";

        var entry = ModuleEntry.Create(_entryId).Value;
        var result = entry.EditContent(content, [textValue, rangeValue]);

        result.IsSuccess.Should().BeTrue();
        entry.Content.Should().Be(content);
        entry.Editablevalues.Should().Contain(textValue).And.Contain(rangeValue);
    }

    [Fact]
    public void ModuleEntry_Should_Fail_EditContent_If_Placeholder_Missing()
    {
        var textValue = TextValue.Create(new EntryValueId(Guid.NewGuid()), "init", "Title", _placeholder1).Value;
        var content = "# Nix content without placeholders";

        var entry = ModuleEntry.Create(_entryId).Value;
        var result = entry.EditContent(content, [textValue]);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain(textValue.Name);
    }

    [Fact]
    public void ModuleEntry_Should_Clear_Previous_Values_When_Editing()
    {
        var textValue1 = TextValue.Create(new EntryValueId(Guid.NewGuid()), "init", "One", _placeholder1).Value;
        var textValue2 = TextValue.Create(new EntryValueId(Guid.NewGuid()), "init", "Two", _placeholder2).Value;
        var content1 = $"placeholder: {_placeholder1}";
        var content2 = $"placeholder: {_placeholder2}";

        var entry = ModuleEntry.Create(_entryId).Value;

        entry.EditContent(content1, [textValue1]);
        entry.Editablevalues.Should().ContainSingle();

        entry.EditContent(content2, [textValue2]);
        entry.Editablevalues.Should().ContainSingle().And.Contain(textValue2);
    }
}
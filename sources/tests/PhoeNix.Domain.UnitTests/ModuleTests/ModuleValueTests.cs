using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class ModuleValueTests
{
    private readonly ModuleValueId _id = new(Guid.NewGuid());
    private readonly ModuleTemplateId _templateId = new(Guid.NewGuid());

    [Fact]
    public void ModuleValue_Should_Create_Enabled()
    {
        var result = ModuleValue.Create(_id, _templateId, enabled: true);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_id);
        result.Value.ModuleTemplateId.Should().Be(_templateId);
        result.Value.Enabled.Should().BeTrue();
        result.Value.EditableValues.Should().BeEmpty();
    }

    [Fact]
    public void ModuleValue_Should_Create_Disabled()
    {
        var result = ModuleValue.Create(_id, _templateId, enabled: false);

        result.IsSuccess.Should().BeTrue();
        result.Value.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ModuleValue_Should_Enable()
    {
        var mv = ModuleValue.Create(_id, _templateId, enabled: false).Value;

        var result = mv.Enable();

        result.IsSuccess.Should().BeTrue();
        mv.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ModuleValue_Enable_Should_Be_Idempotent()
    {
        var mv = ModuleValue.Create(_id, _templateId, enabled: true).Value;

        var result = mv.Enable();

        result.IsSuccess.Should().BeTrue();
        mv.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ModuleValue_Should_Disable()
    {
        var mv = ModuleValue.Create(_id, _templateId, enabled: true).Value;

        var result = mv.Disable();

        result.IsSuccess.Should().BeTrue();
        mv.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ModuleValue_Disable_Should_Be_Idempotent()
    {
        var mv = ModuleValue.Create(_id, _templateId, enabled: false).Value;

        var result = mv.Disable();

        result.IsSuccess.Should().BeTrue();
        mv.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ModuleValue_Should_SetEnabled_True()
    {
        var mv = ModuleValue.Create(_id, _templateId, enabled: false).Value;

        mv.SetEnabled(true).IsSuccess.Should().BeTrue();
        mv.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ModuleValue_Should_SetEnabled_False()
    {
        var mv = ModuleValue.Create(_id, _templateId, enabled: true).Value;

        mv.SetEnabled(false).IsSuccess.Should().BeTrue();
        mv.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ModuleValue_Should_ReplaceEntries()
    {
        var mv = ModuleValue.Create(_id, _templateId).Value;
        var entries = new List<EntryValue>
        {
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "v1", "Entry1", "PH1").Value,
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "v2", "Entry2", "PH2").Value
        };

        var result = mv.ReplaceEntries(entries);

        result.IsSuccess.Should().BeTrue();
        mv.EditableValues.Should().HaveCount(2);
    }

    [Fact]
    public void ModuleValue_ReplaceEntries_Should_Overwrite_Existing()
    {
        var mv = ModuleValue.Create(_id, _templateId).Value;
        mv.ReplaceEntries(new[] { TextValue.Create(new EntryValueId(Guid.NewGuid()), "old", "Old", "OLD").Value });

        var newEntries = new[] { TextValue.Create(new EntryValueId(Guid.NewGuid()), "new", "New", "NEW").Value };
        var result = mv.ReplaceEntries(newEntries);

        result.IsSuccess.Should().BeTrue();
        mv.EditableValues.Should().ContainSingle(e => e.Name == "New");
    }

    [Fact]
    public void ModuleValue_Should_Fail_ReplaceEntries_With_Duplicate_Names()
    {
        var mv = ModuleValue.Create(_id, _templateId).Value;
        var entries = new List<EntryValue>
        {
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "v1", "DupName", "PH1").Value,
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "v2", "DupName", "PH2").Value
        };

        var result = mv.ReplaceEntries(entries);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("unique");
    }

    [Fact]
    public void ModuleValue_Should_Fail_ReplaceEntries_With_Duplicate_Placeholders()
    {
        var mv = ModuleValue.Create(_id, _templateId).Value;
        var entries = new List<EntryValue>
        {
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "v1", "Name1", "SAME_PH").Value,
            TextValue.Create(new EntryValueId(Guid.NewGuid()), "v2", "Name2", "SAME_PH").Value
        };

        var result = mv.ReplaceEntries(entries);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("unique");
    }
}

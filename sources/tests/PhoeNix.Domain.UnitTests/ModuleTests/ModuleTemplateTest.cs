using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class ModuleTemplateTests
{
    private readonly ModuleTemplateId _moduleTemplateId = new(Guid.NewGuid());

    private readonly Architecture _arch1 = Architecture.X86Linux;
    private readonly Architecture _arch2 = Architecture.Aarch64Linux;

    [Theory]
    [InlineData(ModuleType.Generic)]
    [InlineData(ModuleType.System)]
    public void ModuleTemplate_Should_Create_Successfully(ModuleType moduleType)
    {
        var result = ModuleTemplate.Create(
            _moduleTemplateId,
            "TestModule",
            true,
            moduleType,
            new List<Architecture> { _arch1 });

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_moduleTemplateId);
        result.Value.Name.Should().Be("TestModule");
        result.Value.Type.Should().Be(moduleType);
        result.Value.Content.Should().BeEmpty();
        result.Value.SupportedArchitectures.Should().ContainSingle().And.Contain(_arch1);
        result.Value.Tests.Should().BeEmpty();
        result.Value.EditableValueTypes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ModuleType.Generic)]
    [InlineData(ModuleType.System)]
    public void ModuleTemplate_Should_Fail_Create_When_Name_Empty(ModuleType moduleType)
    {
        var result = ModuleTemplate.Create(
            _moduleTemplateId,
            string.Empty,
            true,
            moduleType,
            new List<Architecture> { _arch1 });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Modules name can't be empty");
    }

    [Theory]
    [InlineData(ModuleType.Generic)]
    [InlineData(ModuleType.System)]
    public void ModuleTemplate_Should_Fail_Create_When_Architectures_Empty(ModuleType moduleType)
    {
        var result = ModuleTemplate.Create(
            _moduleTemplateId,
            "ValidName",
            true,
            moduleType,
            new List<Architecture>());

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module has to support at least one architecture");
    }

    [Fact]
    public void ModuleTemplate_Should_Edit_Name()
    {
        var module = CreateValidModule();

        var result = module.EditModule("Updated");

        result.IsSuccess.Should().BeTrue();
        module.Name.Should().Be("Updated");
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_Edit_Name_When_Empty()
    {
        var module = CreateValidModule();

        var result = module.EditModule(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module name can't be empty");
        module.Name.Should().Be("ValidModule");
    }

    [Fact]
    public void ModuleTemplate_Should_Change_Content_And_Set_EditableValueTypes()
    {
        var module = CreateValidModule();

        var entry1 = CreateEntryDefinition("VALUE_ONE", "{VALUE_ONE}", UserInputType.Text);
        var entry2 = CreateEntryDefinition("VALUE_TWO", "{VALUE_TWO}", UserInputType.Text);

        var content = "some text VALUE_ONE and also VALUE_TWO";

        var result = module.ChangeContent(content, new List<EntryValueDefinition> { entry1, entry2 });

        result.IsSuccess.Should().BeTrue();
        module.Content.Should().Be(content);
        module.EditableValueTypes.Should().HaveCount(2);
        module.EditableValueTypes.Should().Contain(e => e.Name == "VALUE_ONE");
        module.EditableValueTypes.Should().Contain(e => e.Name == "VALUE_TWO");
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_ChangeContent_When_Entry_Name_Not_Present_In_Content()
    {
        var module = CreateValidModule();

        var entry = CreateEntryDefinition("MUST_BE_PRESENT", "{MUST_BE_PRESENT}", UserInputType.Text);
        var content = "this does not include the token";

        var result = module.ChangeContent(content, new List<EntryValueDefinition> { entry });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Name for value MUST_BE_PRESENT is not present");

        module.Content.Should().BeEmpty();
        module.EditableValueTypes.Should().BeEmpty();
    }

    [Fact]
    public void ModuleTemplate_Should_Replace_EditableValueTypes_On_Subsequent_ChangeContent()
    {
        var module = CreateValidModule();

        var entry1 = CreateEntryDefinition("ONE", "{ONE}", UserInputType.Text);
        module.ChangeContent("ONE", new List<EntryValueDefinition> { entry1 });

        var entry2 = CreateEntryDefinition("TWO", "{TWO}", UserInputType.Text);
        var result = module.ChangeContent("TWO", new List<EntryValueDefinition> { entry2 });

        result.IsSuccess.Should().BeTrue();
        module.EditableValueTypes.Should().ContainSingle(e => e.Name == "TWO");
        module.EditableValueTypes.Should().NotContain(e => e.Name == "ONE");
    }

    [Fact]
    public void ModuleTemplate_Should_Add_Architecture_Support()
    {
        var module = CreateValidModule();

        var result = module.AddArchitectureSupport(_arch2);

        result.IsSuccess.Should().BeTrue();
        module.SupportedArchitectures.Should().Contain(_arch2);
    }

    [Fact]
    public void ModuleTemplate_Should_Not_Add_Existing_Architecture()
    {
        var module = CreateValidModule();

        var result = module.AddArchitectureSupport(_arch1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("", $"Can't add already supported architecture {_arch1}"));
    }

    [Fact]
    public void ModuleTemplate_Should_Add_Architectures_Support()
    {
        var module = CreateValidModule();

        var result = module.AddArchitecturesSupport(new[] { _arch2 });

        result.IsSuccess.Should().BeTrue();
        module.SupportedArchitectures.Should().Contain(_arch2);
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_Add_Architectures_Support_When_Any_Already_Supported()
    {
        var module = CreateValidModule();

        var result = module.AddArchitecturesSupport(new[] { _arch1, _arch2 });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Can't add already supported architectures");

        module.SupportedArchitectures.Should().ContainSingle().And.Contain(_arch1);
        module.SupportedArchitectures.Should().NotContain(_arch2);
    }

    [Fact]
    public void ModuleTemplate_Should_Remove_Architecture_Support()
    {
        var module = CreateValidModule();

        var result = module.RemoveArchitectureSupport(_arch1);

        result.IsSuccess.Should().BeTrue();
        module.SupportedArchitectures.Should().NotContain(_arch1);
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_To_Remove_Nonexistent_Architecture()
    {
        var module = CreateValidModule();

        var result = module.RemoveArchitectureSupport(_arch2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.ValueNotFound);
    }

    [Fact]
    public void ModuleTemplate_AddModuleTest_Currently_Does_Not_Add_To_Tests_List()
    {
        var module = CreateValidModule();

        var result = module.AddModuleTest("test01");

        result.IsSuccess.Should().BeTrue();

        module.Tests.Should().HaveCount(1);
    }

    // Helpers

    private ModuleTemplate CreateValidModule()
    {
        return ModuleTemplate
            .Create(_moduleTemplateId, "ValidModule", true, ModuleType.Generic, new List<Architecture> { _arch1 })
            .Value;
    }

    private EntryValueDefinition CreateEntryDefinition(string name, string placeholder, UserInputType inputType)
    {
        return new EntryValueDefinition(
            _moduleTemplateId,
            name,
            placeholder,
            inputType);
    }
}
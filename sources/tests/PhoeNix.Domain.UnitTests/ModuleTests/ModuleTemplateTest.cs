using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;

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
        result.Error.Description.Should().Be("Module template name can't be empty.");
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
        result.Error.Description.Should().Be("Module template has to support at least one architecture.");
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
        result.Error.Description.Should().Be("Module template name can't be empty.");
        module.Name.Should().Be("ValidModule");
    }

    [Fact]
    public void ModuleTemplate_Should_Change_Content_And_Set_EditableValueTypes()
    {
        var module = CreateValidModule();

        var entry1 = CreateEntryDefinition("VALUE_ONE", "VALUE_ONE");
        var entry2 = CreateEntryDefinition("VALUE_TWO", "VALUE_TWO");

        var content = "some text VALUE_ONE and also VALUE_TWO";

        var result = module.ChangeContent(content, new List<EntryValueDefinition> { entry1, entry2 });

        result.IsSuccess.Should().BeTrue();
        module.Content.Should().Be(content);
        module.EditableValueTypes.Should().HaveCount(2);
        module.EditableValueTypes.Should().Contain(e => e.Name == "VALUE_ONE");
        module.EditableValueTypes.Should().Contain(e => e.Name == "VALUE_TWO");
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_ChangeContent_When_Content_Empty()
    {
        var module = CreateValidModule();

        var result = module.ChangeContent(string.Empty, new List<EntryValueDefinition>());

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module template content can't be empty.");
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_ChangeContent_When_Duplicate_Entry_Names()
    {
        var module = CreateValidModule();
        var entry1 = CreateEntryDefinition("DUP", "DUP1");
        var entry2 = CreateEntryDefinition("DUP", "DUP2");

        var result = module.ChangeContent("DUP1 DUP2", new List<EntryValueDefinition> { entry1, entry2 });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("unique");
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_ChangeContent_When_Duplicate_Placeholders()
    {
        var module = CreateValidModule();
        var entry1 = CreateEntryDefinition("NAME1", "SAMEPH");
        var entry2 = CreateEntryDefinition("NAME2", "SAMEPH");

        var result = module.ChangeContent("SAMEPH", new List<EntryValueDefinition> { entry1, entry2 });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("unique");
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_ChangeContent_When_Entry_Placeholder_Not_Present_In_Content()
    {
        var module = CreateValidModule();

        var entry = CreateEntryDefinition("Must Be Present", "MUST_BE_PRESENT");
        var content = "this does not include the token";

        var result = module.ChangeContent(content, new List<EntryValueDefinition> { entry });

        result.IsFailure.Should().BeTrue();

        module.Content.Should().BeEmpty();
        module.EditableValueTypes.Should().BeEmpty();
    }

    [Fact]
    public void ModuleTemplate_Should_Replace_EditableValueTypes_On_Subsequent_ChangeContent()
    {
        var module = CreateValidModule();

        var entry1 = CreateEntryDefinition("ONE", "ONE");
        module.ChangeContent("ONE", new List<EntryValueDefinition> { entry1 });

        var entry2 = CreateEntryDefinition("TWO", "TWO");
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
        result.Error.Code.Should().Be("Modules.ArchitectureAlreadySupported");
        result.Error.Description.Should().Be($"Architecture '{_arch1}' is already supported.");
    }

    [Fact]
    public void ModuleTemplate_Should_Replace_Architecture_Support()
    {
        var module = CreateValidModule();

        var result = module.ReplaceArchitectureSupport(new[] { _arch2 });

        result.IsSuccess.Should().BeTrue();
        module.SupportedArchitectures.Should().ContainSingle().And.Contain(_arch2);
        module.SupportedArchitectures.Should().NotContain(_arch1);
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_ReplaceArchitectureSupport_When_Empty()
    {
        var module = CreateValidModule();

        var result = module.ReplaceArchitectureSupport(Array.Empty<Architecture>());

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module template has to support at least one architecture.");
        module.SupportedArchitectures.Should().ContainSingle().And.Contain(_arch1);
    }

    [Fact]
    public void ModuleTemplate_Should_SetEnabled()
    {
        var module = CreateValidModule();

        module.SetEnabled(false).IsSuccess.Should().BeTrue();
        module.Enabled.Should().BeFalse();

        module.SetEnabled(true).IsSuccess.Should().BeTrue();
        module.Enabled.Should().BeTrue();
    }

    [Fact]
    public void ModuleTemplate_Should_ChangeType()
    {
        var module = CreateValidModule();

        module.ChangeType(ModuleType.System).IsSuccess.Should().BeTrue();
        module.Type.Should().Be(ModuleType.System);
    }

    [Fact]
    public void ModuleTemplate_AddModuleTest_Should_Add_To_Tests_List()
    {
        var module = CreateValidModule();

        var result = module.AddModuleTest("test01");

        result.IsSuccess.Should().BeTrue();
        module.Tests.Should().HaveCount(1);
    }

    [Fact]
    public void ModuleTemplate_AddModuleTest_Should_Fail_When_Duplicate_Name()
    {
        var module = CreateValidModule();
        module.AddModuleTest("test01");

        var result = module.AddModuleTest("test01");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already exists");
        module.Tests.Should().HaveCount(1);
    }

    [Fact]
    public void ModuleTemplate_RemoveModuleTest_Should_Remove_Test()
    {
        var module = CreateValidModule();
        module.AddModuleTest("test01");
        var testId = module.Tests.Single().Id;

        var result = module.RemoveModuleTest(testId);

        result.IsSuccess.Should().BeTrue();
        module.Tests.Should().BeEmpty();
    }

    [Fact]
    public void ModuleTemplate_RemoveModuleTest_Should_Fail_When_Not_Found()
    {
        var module = CreateValidModule();

        var result = module.RemoveModuleTest(new TestId(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no module test with id");
    }

    [Fact]
    public void ModuleTemplate_ChangeModuleTest_Should_Update_Content_And_Variables()
    {
        var module = CreateValidModule();
        module.AddModuleTest("test01");
        var testId = module.Tests.Single().Id;

        var result = module.ChangeModuleTest(testId, "echo VAR1", new List<string> { "VAR1" });

        result.IsSuccess.Should().BeTrue();
        module.Tests.Single().Content.Should().Be("echo VAR1");
        module.Tests.Single().VariableNames.Should().ContainSingle("VAR1");
    }

    [Fact]
    public void ModuleTemplate_ChangeModuleTest_Should_Fail_When_Not_Found()
    {
        var module = CreateValidModule();

        var result = module.ChangeModuleTest(new TestId(Guid.NewGuid()), "echo x", new List<string>());

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("has not been found");
    }

    [Fact]
    public void ModuleTemplate_SetRequiredInputs_Should_Set_And_Clear()
    {
        var module = CreateValidModule();

        var result = module.SetRequiredInputs(new[] { ("nixpkgs", "github:nixos/nixpkgs") });

        result.IsSuccess.Should().BeTrue();
        module.RequiredInputs.Should().ContainSingle(i => i.Name == "nixpkgs");

        module.SetRequiredInputs(Array.Empty<(string, string)>());
        module.RequiredInputs.Should().BeEmpty();
    }

    [Fact]
    public void ModuleTemplate_ReconcileTests_Should_Add_New_Tests()
    {
        var module = CreateValidModule();
        var def = new ModuleTemplateTestDefinition(null, "newtest", "echo hello", new List<string>());

        var result = module.ReconcileTests(new[] { def });

        result.IsSuccess.Should().BeTrue();
        module.Tests.Should().ContainSingle(t => t.Name == "newtest");
    }

    [Fact]
    public void ModuleTemplate_ReconcileTests_Should_Remove_Unlisted_Tests()
    {
        var module = CreateValidModule();
        module.AddModuleTest("keeptest");
        module.AddModuleTest("removetest");
        var keepId = module.Tests.First(t => t.Name == "keeptest").Id;

        var result = module.ReconcileTests(new[]
        {
            new ModuleTemplateTestDefinition(keepId, "keeptest", "echo keep", new List<string>())
        });

        result.IsSuccess.Should().BeTrue();
        module.Tests.Should().ContainSingle(t => t.Name == "keeptest");
        module.Tests.Should().NotContain(t => t.Name == "removetest");
    }

    [Fact]
    public void ModuleTemplate_ReconcileTests_Should_Fail_On_Duplicate_Names()
    {
        var module = CreateValidModule();

        var result = module.ReconcileTests(new[]
        {
            new ModuleTemplateTestDefinition(null, "dup", "echo a", new List<string>()),
            new ModuleTemplateTestDefinition(null, "dup", "echo b", new List<string>())
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("unique");
    }

    [Fact]
    public void ModuleTemplate_ReconcileTests_Should_Fail_When_Id_Not_Found()
    {
        var module = CreateValidModule();
        var nonExistentId = new TestId(Guid.NewGuid());

        var result = module.ReconcileTests(new[]
        {
            new ModuleTemplateTestDefinition(nonExistentId, "ghost", "echo x", new List<string>())
        });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("not found");
    }

    [Fact]
    public void ModuleTemplate_Should_Fail_ChangeContent_When_Entry_Has_No_Options_For_SingleChoice()
    {
        var module = CreateValidModule();
        var entry = new EntryValueDefinition(
            _moduleTemplateId,
            "CHOICE",
            "CHOICE",
            EntryBindingKind.UserProvided,
            EntryValueKind.SingleChoice);

        var result = module.ChangeContent("CHOICE", new List<EntryValueDefinition> { entry });

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("requires at least one option");
    }

    private ModuleTemplate CreateValidModule()
    {
        return ModuleTemplate
            .Create(_moduleTemplateId, "ValidModule", true, ModuleType.Generic, new List<Architecture> { _arch1 })
            .Value;
    }

    private EntryValueDefinition CreateEntryDefinition(string name, string placeholder)
    {
        return new EntryValueDefinition(
            _moduleTemplateId,
            name,
            placeholder,
            EntryBindingKind.UserProvided,
            EntryValueKind.Text);
    }
}
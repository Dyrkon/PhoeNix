using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Shared;
using Xunit.Abstractions;

namespace PhoeNix.Domain.UnitTests.ModuleTests;

public class ModuleTest
{
    private readonly ITestOutputHelper _testOutputHelper;
    private readonly ModuleId ModuleId1 = new(Guid.NewGuid());
    private readonly EntryValueId EntryId1 = new(Guid.NewGuid());
    private readonly Architecture Arch1 = Architecture.X86Linux;
    private readonly Architecture Arch2 = Architecture.Aarch64Linux;

    public ModuleTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [InlineData(ModuleType.Generic)]
    [InlineData(ModuleType.Home)]
    [InlineData(ModuleType.System)]
    public void Module_Should_Create_Successfully(ModuleType moduleType)
    {
        var module = Module.Create(ModuleId1, "TestModule", true, moduleType, [Arch1]);

        module.IsSuccess.Should().BeTrue();
        module.Value.Name.Should().Be("TestModule");
        module.Value.Type.Should().Be(moduleType);
        module.Value.Enabled.Should().BeTrue();
        module.Value.SupportedArchitectures.Should().ContainSingle().And.Contain(Arch1);
    }

    [Theory]
    [InlineData(ModuleType.Generic)]
    [InlineData(ModuleType.Home)]
    [InlineData(ModuleType.System)]
    public void Module_Should_Fail_Create_When_Name_Empty(ModuleType moduleType)
    {
        var result = Module.Create(ModuleId1, string.Empty, true, moduleType, [Arch1]);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Modules name can't be empty");
    }

    [Theory]
    [InlineData(ModuleType.Generic)]
    [InlineData(ModuleType.Home)]
    [InlineData(ModuleType.System)]
    public void Module_Should_Fail_Create_When_Architectures_Empty(ModuleType moduleType)
    {
        var result = Module.Create(ModuleId1, "ValidName", true, moduleType, []);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module has to support at least one architecture");
    }

    [Fact]
    public void Module_Should_Add_Entry()
    {
        var module = CreateValidModule();
        var entry = TextValue.Create(new EntryValueId(Guid.NewGuid()), "Something", "Foo").Value;
        var result = module.ChangeContent("Value = Something", [entry]);

        result.IsSuccess.Should().BeTrue();
        module.EditableValues.Should().Contain(entry);
    }

    [Fact]
    public void Module_Should_Not_Add_Same_Entry_Twice()
    {
        var module = CreateValidModule();
        var entry = TextValue.Create(new EntryValueId(Guid.NewGuid()), "Init", "Foo").Value;

        module.ChangeContent("Something = Init", [entry]);
        var result = module.AddEntry(entry);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Can't add editable value twice");
    }

    [Fact]
    public void Module_Should_Remove_Entry()
    {
        var module = CreateValidModule();
        var entry = TextValue.Create(new EntryValueId(Guid.NewGuid()), "Init", "Foo").Value;

        module.ChangeContent("Value = Init", [entry]);
        var result = module.RemoveEntry(entry.Id);

        result.IsSuccess.Should().BeTrue();
        module.EditableValues.Should().BeEmpty();
    }

    [Fact]
    public void Module_Should_Fail_To_Remove_Nonexistent_Entry()
    {
        var module = CreateValidModule();

        var result = module.RemoveEntry(EntryId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"Entry not present in module");
    }

    [Fact]
    public void Module_Should_Enable_And_Disable()
    {
        var module = CreateValidModule(false);

        var enableResult = module.Enable();
        enableResult.IsSuccess.Should().BeTrue();
        module.Enabled.Should().BeTrue();

        var disableResult = module.Disable();
        disableResult.IsSuccess.Should().BeTrue();
        module.Enabled.Should().BeFalse();
    }

    [Fact]
    public void Module_Should_Not_Enable_Already_Enabled_Module()
    {
        var module = CreateValidModule();

        var result = module.Enable();

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"Module {module.Name} is already enabled");
    }

    [Fact]
    public void Module_Should_Not_Disable_Already_Disabled_Module()
    {
        var module = CreateValidModule(false);

        var result = module.Disable();

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"Module {module.Name} is already disabled");
    }

    [Theory]
    [InlineData("")]
    public void Module_Should_Fail_EditModule_When_Name_Is_Empty(string newName)
    {
        var module = CreateValidModule();

        var result = module.EditModule(newName);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Module name can't be empty");
    }

    [Fact]
    public void Module_Should_Edit_Name()
    {
        var module = CreateValidModule();
        var result = module.EditModule("Updated");

        result.IsSuccess.Should().BeTrue();
        module.Name.Should().Be("Updated");
    }

    [Fact]
    public void Module_Should_Add_Architecture_Support()
    {
        var module = CreateValidModule();

        var result = module.AddArchitectureSupport(Arch2);

        result.IsSuccess.Should().BeTrue();
        module.SupportedArchitectures.Should().Contain(Arch2);
    }

    [Fact]
    public void Module_Should_Not_Add_Existing_Architecture()
    {
        var module = CreateValidModule();

        var result = module.AddArchitectureSupport(Arch1);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(new Error("", $"Can't add already supported architecture {Arch1}"));
    }

    [Fact]
    public void Module_Should_Remove_Architecture_Support()
    {
        var module = CreateValidModule();

        var result = module.RemoveArchitectureSupport(Arch1);

        result.IsSuccess.Should().BeTrue();
        module.SupportedArchitectures.Should().NotContain(Arch1);
    }

    [Fact]
    public void Module_Should_Fail_To_Remove_Nonexistent_Architecture()
    {
        var module = CreateValidModule();
        var result = module.RemoveArchitectureSupport(Arch2);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.ValueNotFound);
    }

    // Helper
    private Module CreateValidModule(bool enabled = true)
    {
        return Module.Create(ModuleId1, "ValidModule", enabled, ModuleType.Generic, [Arch1])
            .Value;
    }
}
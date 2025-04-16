using FluentAssertions;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.UnitTests;

public class SystemTests
{
    private readonly SystemId systemId = new(Guid.NewGuid());
    private readonly ModuleId moduleId1 = new(Guid.NewGuid());
    private readonly ModuleId moduleId2 = new(Guid.NewGuid());
    private readonly string name1 = "Some name 1";
    private readonly string name2 = "Some name 2";
    private readonly Architecture compatibleArch = Architecture.X86Linux;
    private readonly Architecture incompatibleArch = Architecture.Aarch64Darwin;

    [Fact]
    public void System_Should_Create_Successfully()
    {
        var result = Entities.Systems.System.Create(systemId, compatibleArch, name1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Architecture.Should().Be(compatibleArch);
        result.Value.Modules.Should().BeEmpty();
    }

    [Fact]
    public void System_Should_Add_Compatible_Module()
    {
        var system = Entities.Systems.System.Create(systemId, compatibleArch, name1).Value;
        var module = CreateModuleWithArch(moduleId1, compatibleArch);

        var result = system.AddModule(module);

        result.IsSuccess.Should().BeTrue();
        system.Modules.Should().ContainSingle(m => m.ModuleId == moduleId1);
    }

    [Fact]
    public void System_Should_Change_Name()
    {
        var system = Entities.Systems.System.Create(systemId, compatibleArch, name1).Value;

        var result = system.ChangeName(name2);

        result.IsSuccess.Should().BeTrue();
        system.Name.Should().Be(name2);
    }

    [Fact]
    public void System_Should_Fail_To_Add_Module_Twice()
    {
        var system = Entities.Systems.System.Create(systemId, compatibleArch, name1).Value;
        var module = CreateModuleWithArch(moduleId1, compatibleArch);

        system.AddModule(module);
        var result = system.AddModule(module);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("This module has been added to this system already");
    }

    [Fact]
    public void System_Should_Fail_To_Add_Incompatible_Module()
    {
        var system = Entities.Systems.System.Create(systemId, compatibleArch, name1).Value;
        var incompatibleModule = CreateModuleWithArch(moduleId2, incompatibleArch);

        var result = system.AddModule(incompatibleModule);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"This module doesn't support system architecture {compatibleArch}");
    }

    [Fact]
    public void System_Should_Remove_Module()
    {
        var system = Entities.Systems.System.Create(systemId, compatibleArch, name1).Value;
        var module = CreateModuleWithArch(moduleId1, compatibleArch);

        system.AddModule(module);
        var result = system.RemoveModule(moduleId1);

        result.IsSuccess.Should().BeTrue();
        system.Modules.Should().BeEmpty();
    }

    [Fact]
    public void System_Should_Fail_To_Remove_Nonexistent_Module()
    {
        var system = Entities.Systems.System.Create(systemId, compatibleArch, name1).Value;

        var result = system.RemoveModule(moduleId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"There is no module with id {moduleId1} in this system");
    }

    // Helper
    private Module CreateModuleWithArch(ModuleId id, Architecture architecture)
    {
        return Module.Create(id, "TestModule", true, ModuleType.Generic, [architecture]).Value;
    }
}
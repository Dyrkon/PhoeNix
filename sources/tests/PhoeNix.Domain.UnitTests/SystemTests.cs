using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.UnitTests;

public class SystemTests
{
    private readonly ConfigurationId _configurationId = new(Guid.NewGuid());
    private readonly SystemId _systemId = new(Guid.NewGuid());

    private readonly ModuleTemplateId _moduleTemplateId1 = new(Guid.NewGuid());
    private readonly ModuleTemplateId _moduleTemplateId2 = new(Guid.NewGuid());

    private const string Name1 = "Some name 1";
    private const string Name2 = "Some name 2";

    private readonly Architecture _compatibleArch = Architecture.X86Linux;
    private readonly Architecture _incompatibleArch = Architecture.Aarch64Darwin;

    [Fact]
    public void System_Should_Create_Successfully()
    {
        var result = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(_systemId);
        result.Value.Architecture.Should().Be(_compatibleArch);
        result.Value.Name.Should().Be(Name1);
        result.Value.Modules.Should().BeEmpty();
    }

    [Fact]
    public void System_Should_Change_Name()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        var result = system.ChangeName(Name2);

        result.IsSuccess.Should().BeTrue();
        system.Name.Should().Be(Name2);
    }

    [Fact]
    public void System_Should_Fail_To_Change_Name_When_Empty()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        var result = system.ChangeName(string.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("System name can't be empty.");
        system.Name.Should().Be(Name1);
    }

    [Fact]
    public void System_Should_Add_Compatible_Module()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        var result = system.AddModule(
            _moduleTemplateId1,
            new List<Architecture> { _compatibleArch },
            true);

        result.IsSuccess.Should().BeTrue();
        system.Modules.Should().ContainSingle(m => m.ModuleTemplateId == _moduleTemplateId1);

        var added = system.Modules.Single(m => m.ModuleTemplateId == _moduleTemplateId1);
        added.Enabled.Should().BeTrue();
    }

    [Fact]
    public void System_Should_Add_Module_Disabled_When_Requested()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        var result = system.AddModule(
            _moduleTemplateId1,
            new List<Architecture> { _compatibleArch },
            false);

        result.IsSuccess.Should().BeTrue();
        system.Modules.Should().ContainSingle(m => m.ModuleTemplateId == _moduleTemplateId1);

        var added = system.Modules.Single(m => m.ModuleTemplateId == _moduleTemplateId1);
        added.Enabled.Should().BeFalse();
    }

    [Fact]
    public void System_Should_Fail_To_Add_Module_Twice()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        system.AddModule(
            _moduleTemplateId1,
            new List<Architecture> { _compatibleArch },
            true);

        var result = system.AddModule(
            _moduleTemplateId1,
            new List<Architecture> { _compatibleArch },
            true);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("This module has already been added to this system.");
        system.Modules.Should().ContainSingle(m => m.ModuleTemplateId == _moduleTemplateId1);
    }

    [Fact]
    public void System_Should_Fail_To_Add_Incompatible_Module()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        var result = system.AddModule(
            _moduleTemplateId2,
            new List<Architecture> { _incompatibleArch },
            true);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"This module doesn't support system architecture '{_compatibleArch}'.");
        system.Modules.Should().BeEmpty();
    }

    [Fact]
    public void System_Should_Remove_Module_By_ModuleValueId()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        system.AddModule(
            _moduleTemplateId1,
            new List<Architecture> { _compatibleArch },
            true);

        var moduleValueId = system.Modules.Single(m => m.ModuleTemplateId == _moduleTemplateId1).Id;

        var result = system.RemoveModule(moduleValueId);

        result.IsSuccess.Should().BeTrue();
        system.Modules.Should().BeEmpty();
    }

    [Fact]
    public void System_Should_Fail_To_Remove_Nonexistent_Module()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        var result = system.RemoveModule(new ModuleValueId(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no module with id");
        result.Error.Description.Should().Contain("in this system");
    }

    [Fact]
    public void System_Should_Update_Module_Enabled_State()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;
        system.AddModule(_moduleTemplateId1, new List<Architecture> { _compatibleArch }, true);

        var moduleValueId = system.Modules.Single().Id;
        var result = system.UpdateModule(moduleValueId, false, new List<EntryValue>());

        result.IsSuccess.Should().BeTrue();
        system.Modules.Single().Enabled.Should().BeFalse();
    }

    [Fact]
    public void System_Should_Fail_Update_Module_When_Not_Found()
    {
        var system = Entities.Systems.System.Create(_systemId, _configurationId, _compatibleArch, Name1).Value;

        var result = system.UpdateModule(new ModuleValueId(Guid.NewGuid()), false, new List<EntryValue>());

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no module with id");
    }
}

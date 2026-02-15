using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.UnitTests;

public class ConfigurationTests
{
    private readonly ConfigurationId _configId = new(Guid.NewGuid());
    private const string Title = "MyConfig";
    private const string Description = "Config description";

    private readonly ModuleTemplateId _moduleTemplateId1 = new(Guid.NewGuid());
    private readonly ModuleTemplateId _moduleTemplateId2 = new(Guid.NewGuid());

    private readonly SystemId _systemId1 = new(Guid.NewGuid());
    private readonly SystemId _systemId2 = new(Guid.NewGuid());

    [Fact]
    public void Configuration_Should_Create_Successfully()
    {
        var result = Configuration.Create(_configId, Title, Description);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(Title);
        result.Value.Description.Should().Be(Description);
        result.Value.Inputs.Should().BeEmpty();
        result.Value.Modules.Should().BeEmpty();
        result.Value.SystemSpecifications.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Edit_Title_And_Description()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.EditConfiguration("NewTitle", "NewDescription");

        result.IsSuccess.Should().BeTrue();
        config.Title.Should().Be("NewTitle");
        config.Description.Should().Be("NewDescription");
    }

    [Theory]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void Configuration_Should_Fail_Edit_When_Empty(string? newTitle, string? newDescription)
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.EditConfiguration(newTitle, newDescription);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Title can't be blank");
    }

    [Fact]
    public void Configuration_Should_Add_Module()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.AddModule(_moduleTemplateId1, true);

        result.IsSuccess.Should().BeTrue();
        config.Modules.Should().ContainSingle(m => m.ModuleTemplateId == _moduleTemplateId1);
        config.Modules.Single(m => m.ModuleTemplateId == _moduleTemplateId1).Enabled.Should().BeTrue();
    }

    [Fact]
    public void Configuration_Should_Not_Add_Duplicate_Module()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        config.AddModule(_moduleTemplateId1, true);

        var result = config.AddModule(_moduleTemplateId1, false);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already");
        config.Modules.Should().ContainSingle(m => m.ModuleTemplateId == _moduleTemplateId1);
    }

    [Fact]
    public void Configuration_Should_Remove_Module()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        config.AddModule(_moduleTemplateId1, true);

        // Configuration.RemoveModule takes ModuleValueId, but the entity compares by template id in places.
        // This keeps the test resilient to your ID-wrapper design by converting templateId -> Guid -> ModuleValueId.
        var moduleValueId = new ModuleValueId((Guid)config.Modules.Single().ModuleTemplateId);

        var result = config.RemoveModule(moduleValueId);

        result.IsSuccess.Should().BeTrue();
        config.Modules.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Fail_Remove_Nonexistent_Module()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.RemoveModule(new ModuleValueId(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no module");
    }

    [Fact]
    public void Configuration_Should_Add_System()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.AddSystem(_systemId1, Architecture.X86Linux, "system-one");

        result.IsSuccess.Should().BeTrue();
        config.SystemSpecifications.Should().ContainSingle(s => s.Id == _systemId1);
        config.SystemSpecifications.Single(s => s.Id == _systemId1).Architecture.Should().Be(Architecture.X86Linux);
        config.SystemSpecifications.Single(s => s.Id == _systemId1).Name.Should().Be("system-one");
    }

    [Fact]
    public void Configuration_Should_Not_Add_Duplicate_System()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        config.AddSystem(_systemId1, Architecture.X86Linux, "system-one");

        var result = config.AddSystem(_systemId1, Architecture.X86Linux, "system-one-again");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already");
        config.SystemSpecifications.Should().ContainSingle(s => s.Id == _systemId1);
    }

    [Fact]
    public void Configuration_Should_Remove_System()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        config.AddSystem(_systemId1, Architecture.X86Linux, "system-one");

        var result = config.RemoveSystem(_systemId1);

        result.IsSuccess.Should().BeTrue();
        config.SystemSpecifications.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Fail_Remove_Nonexistent_System()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.RemoveSystem(_systemId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no system");
    }

    [Fact]
    public void Configuration_Should_Change_System_Name()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        config.AddSystem(_systemId1, Architecture.X86Linux, "old-name");

        var result = config.ChangeSystemName(_systemId1, "new-name");

        result.IsSuccess.Should().BeTrue();
        config.SystemSpecifications.Single(s => s.Id == _systemId1).Name.Should().Be("new-name");
    }

    [Fact]
    public void Configuration_Should_Fail_Change_System_Name_When_System_Missing()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.ChangeSystemName(_systemId1, "new-name");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no system");
    }

    [Fact]
    public void Configuration_Should_Fail_Change_System_Name_When_Name_Duplicate()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        config.AddSystem(_systemId1, Architecture.X86Linux, "same");
        config.AddSystem(_systemId2, Architecture.X86Linux, "other");

        var result = config.ChangeSystemName(_systemId2, "same");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already");
        config.SystemSpecifications.Single(s => s.Id == _systemId2).Name.Should().Be("other");
    }

    [Fact]
    public void Configuration_Should_Add_Input()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.AddInput("github:nixos/nixpkgs", "nixpkgs");

        result.IsSuccess.Should().BeTrue();
        config.Inputs.Should().ContainSingle(i => i.Name == "nixpkgs");
        config.Inputs.Single(i => i.Name == "nixpkgs").Source.Should().Be("github:nixos/nixpkgs");
    }

    [Fact]
    public void Configuration_Should_Not_Add_Duplicate_Input_By_Name()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        config.AddInput("github:nixos/nixpkgs", "nixpkgs");

        var result = config.AddInput("github:nixos/nixpkgs", "nixpkgs");

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already");
        config.Inputs.Should().ContainSingle(i => i.Name == "nixpkgs");
    }

    [Fact]
    public void Configuration_Should_Remove_Input()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        var input = config.AddInput("github:nixos/nixpkgs", "nixpkgs").Value;

        var result = config.RemoveInput(input.Id);

        result.IsSuccess.Should().BeTrue();
        config.Inputs.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Fail_Remove_Nonexistent_Input()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.RemoveInput(new InputId(Guid.NewGuid()));

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no input");
    }

    [Fact]
    public void Configuration_Should_Add_And_Remove_Input_Follow()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;
        var input = config.AddInput("github:nixos/nixpkgs", "nixpkgs").Value;

        var addFollow = config.AddInputFollow(input.Id, "flake-utils", "github:numtide/flake-utils");
        addFollow.IsSuccess.Should().BeTrue();

        // Remove by locating the follow id that was added.
        var followId = config.Inputs.Single(i => i.Id == input.Id).Followers.Single().Id;

        var removeFollow = config.RemoveInputFollow(followId);
        removeFollow.IsSuccess.Should().BeTrue();
        config.Inputs.Single(i => i.Id == input.Id).Followers.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Return_Empty_SupportedArchitectures_If_No_Systems()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        var result = config.SupportedSystemArchitectures();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Return_SupportedArchitecture_When_All_Systems_Share_It()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        config.AddSystem(_systemId1, Architecture.X86Linux, "s1");
        config.AddSystem(_systemId2, Architecture.X86Linux, "s2");

        var result = config.SupportedSystemArchitectures();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().And.Contain(Architecture.X86Linux);
    }

    [Fact]
    public void Configuration_Should_Return_Empty_SupportedArchitectures_When_Mixed()
    {
        var config = Configuration.Create(_configId, Title, Description).Value;

        config.AddSystem(_systemId1, Architecture.X86Linux, "s1");
        config.AddSystem(_systemId2, Architecture.Aarch64Linux, "s2");

        var result = config.SupportedSystemArchitectures();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
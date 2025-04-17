using FluentAssertions;
using PhoeNix.Domain.Entities.Configurations;
using PhoeNix.Domain.Entities.Inputs;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Systems;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Domain.UnitTests;

public class ConfigurationTests
{
    private readonly ConfigurationId configId = new(Guid.NewGuid());
    private readonly string title = "MyConfig";
    private readonly string description = "Config description";
    private readonly ModuleId moduleId1 = new(Guid.NewGuid());
    private readonly SystemId systemId1 = new(Guid.NewGuid());
    private readonly HomeId homeId1 = new(Guid.NewGuid());
    private readonly InputId inputId1 = new(Guid.NewGuid());

    [Fact]
    public void Configuration_Should_Create_Successfully()
    {
        var result = Configuration.Create(configId, title, description);

        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(title);
        result.Value.Description.Should().Be(description);
    }

    [Fact]
    public void Configuration_Should_Edit_Title_And_Description()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.EditConfiguration("NewTitle", "NewDescription");

        result.IsSuccess.Should().BeTrue();
        config.Title.Should().Be("NewTitle");
        config.Description.Should().Be("NewDescription");
    }

    [Theory]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData("", "")]
    public void Configuration_Should_Fail_Edit_When_Empty(string? newTitle, string? newDesc)
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.EditConfiguration(newTitle, newDesc);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("Title can't be blank");
    }

    [Fact]
    public void Configuration_Should_Add_Module()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.AddModule(moduleId1);

        result.IsSuccess.Should().BeTrue();
        config.Modules.Should().ContainSingle(m => m.ModuleId == moduleId1);
    }

    [Fact]
    public void Configuration_Should_Not_Add_Duplicate_Module()
    {
        var config = Configuration.Create(configId, title, description).Value;
        config.AddModule(moduleId1);

        var result = config.AddModule(moduleId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already");
    }

    [Fact]
    public void Configuration_Should_Remove_Module()
    {
        var config = Configuration.Create(configId, title, description).Value;
        config.AddModule(moduleId1);

        var result = config.RemoveModule(moduleId1);

        result.IsSuccess.Should().BeTrue();
        config.Modules.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Fail_Remove_Nonexistent_Module()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.RemoveModule(moduleId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("There is no module");
    }

    [Fact]
    public void Configuration_Should_Add_And_Remove_System()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var addResult = config.AddSystem(systemId1);
        var removeResult = config.RemoveSystem(systemId1);

        addResult.IsSuccess.Should().BeTrue();
        removeResult.IsSuccess.Should().BeTrue();
        config.Systems.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Fail_To_Add_Duplicate_System()
    {
        var config = Configuration.Create(configId, title, description).Value;
        config.AddSystem(systemId1);

        var result = config.AddSystem(systemId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already");
    }

    [Fact]
    public void Configuration_Should_Fail_To_Remove_Nonexistent_System()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.RemoveSystem(systemId1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Configuration_Should_Add_And_Remove_Home()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var addResult = config.AddHome(homeId1);
        var removeResult = config.RemoveHome(homeId1);

        addResult.IsSuccess.Should().BeTrue();
        removeResult.IsSuccess.Should().BeTrue();
        config.Homes.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Not_Add_Duplicate_Home()
    {
        var config = Configuration.Create(configId, title, description).Value;
        config.AddHome(homeId1);

        var result = config.AddHome(homeId1);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("already");
    }

    [Fact]
    public void Configuration_Should_Fail_To_Remove_Nonexistent_Home()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.RemoveHome(homeId1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Configuration_Should_Add_And_Remove_Input()
    {
        var input = Input.Create(inputId1, "github:nixos", "nixpkgs").Value;
        var config = Configuration.Create(configId, title, description).Value;

        var addResult = config.AddInput(input);
        var removeResult = config.RemoveInput(inputId1);

        addResult.IsSuccess.Should().BeTrue();
        removeResult.IsSuccess.Should().BeTrue();
        config.Inputs.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Fail_To_Add_Duplicate_Input()
    {
        var input = Input.Create(inputId1, "github:nixos", "nixpkgs").Value;
        var config = Configuration.Create(configId, title, description).Value;
        config.AddInput(input);

        var result = config.AddInput(input);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Configuration_Should_Fail_To_Remove_Nonexistent_Input()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.RemoveInput(inputId1);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Configuration_Should_Return_Empty_SupportedArchitectures_If_No_Systems()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var result = config.SupportedSystemArchitectures();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public void Configuration_Should_Return_Correct_Supported_Architectures()
    {
        var config = Configuration.Create(configId, title, description).Value;

        var system1 = Domain.Entities.Systems.System
            .Create(new SystemId(Guid.NewGuid()), Architecture.X86Linux, "Some name").Value;
        var system2 = Domain.Entities.Systems.System
            .Create(new SystemId(Guid.NewGuid()), Architecture.X86Linux, "Some name").Value;

        InjectSystems(config, [system1, system2]);

        var result = config.SupportedSystemArchitectures();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().And.Contain(Architecture.X86Linux);
    }

    private void InjectSystems(Configuration config, List<Domain.Entities.Systems.System> systems)
    {
        var field = typeof(Configuration).GetField("_systems",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        var configurationSystems = systems
            .Select(sys =>
            {
                var cs = ConfigurationSystem.Create(new ConfigurationSystemId(Guid.NewGuid()), config.Id, sys.Id).Value;
                typeof(ConfigurationSystem)
                    .GetProperty(nameof(ConfigurationSystem.System))!
                    .SetValue(cs, sys);
                return cs;
            })
            .ToList();

        field!.SetValue(config, configurationSystems);
    }
}
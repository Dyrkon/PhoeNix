using System;
using FluentAssertions;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using PhoeNix.Domain.Primitives;

namespace PhoeNix.Domain.UnitTests;

public class HomeTests
{
    private readonly HomeId HomeId1 = new(Guid.NewGuid());
    private readonly ModuleId ModuleId1 = new(Guid.NewGuid());
    private readonly ModuleId ModuleId2 = new(Guid.NewGuid());
    private readonly UserId UserId1 = new(Guid.NewGuid());
    private readonly UserId UserId2 = new(Guid.NewGuid());
    private readonly string HomeName = "MyTestHome";

    [Fact]
    public void Home_Should_Create_Successfully()
    {
        var home = Home.Create(HomeId1, HomeName);

        home.IsSuccess.Should().BeTrue();
        home.Value.Name.Should().Be(HomeName);
        home.Value.Modules.Should().BeEmpty();
        home.Value.Users.Should().BeEmpty();
    }

    [Fact]
    public void Home_Should_Add_Module()
    {
        var module = Module.Create(ModuleId1, "mod1", true, ModuleType.Home, [Architecture.X86Linux]).Value;
        var home = Home.Create(HomeId1, HomeName).Value;

        var result = home.AddModule(module);

        result.IsSuccess.Should().BeTrue();
        home.Modules.Should().ContainSingle(m => m.ModuleId == ModuleId1);
    }

    [Fact]
    public void Home_Should_Remove_Existing_Module()
    {
        var module = Module.Create(ModuleId1, "mod1", true, ModuleType.Home, [Architecture.X86Linux]).Value;
        var home = Home.Create(HomeId1, HomeName).Value;
        home.AddModule(module);

        var result = home.RemoveModule(ModuleId1);

        result.IsSuccess.Should().BeTrue();
        home.Modules.Should().BeEmpty();
    }

    [Fact]
    public void Home_Should_Fail_To_Add_Duplicate_Module()
    {
        var module = Module.Create(ModuleId1, "mod1", true, ModuleType.Home, [Architecture.X86Linux]).Value;
        var home = Home.Create(HomeId1, HomeName).Value;

        home.AddModule(module);
        var result = home.AddModule(module);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("This module has been added to this home already");
    }

    [Fact]
    public void Home_Should_Fail_To_Remove_Module_That_Does_Not_Exist()
    {
        var home = Home.Create(HomeId1, HomeName).Value;

        var result = home.RemoveModule(ModuleId2);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"There is no module with id {ModuleId2} in this home");
    }

    [Fact]
    public void Home_Should_Add_User()
    {
        var user = User.Create(UserId1, "Test name", "Test description", "wheel", true, 1024, Shell.Fish, "./here")
            .Value;
        var home = Home.Create(HomeId1, HomeName).Value;

        var result = home.AddUser(user);

        result.IsSuccess.Should().BeTrue();
        home.Users.Should().ContainSingle(u => u.UserId == UserId1);
    }

    [Fact]
    public void Home_Should_Remove_Existing_User()
    {
        var user = User.Create(UserId1, "Test name", "Test description", "wheel", true, 1024, Shell.Fish, "./here")
            .Value;
        var home = Home.Create(HomeId1, HomeName).Value;
        home.AddUser(user);

        var result = home.RemoveUser(UserId1);

        result.IsSuccess.Should().BeTrue();
        home.Users.Should().BeEmpty();
    }

    [Fact]
    public void Home_Should_Fail_To_Add_Duplicate_User()
    {
        var user = User.Create(UserId1, "Test name", "Test description", "wheel", true, 1024, Shell.Fish, "./here")
            .Value;
        var home = Home.Create(HomeId1, HomeName).Value;

        home.AddUser(user);
        var result = home.AddUser(user);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be("This user has been added to this home already");
    }

    [Fact]
    public void Home_Should_Fail_To_Remove_User_That_Does_Not_Exist()
    {
        var home = Home.Create(HomeId1, HomeName).Value;

        var result = home.RemoveUser(UserId2);

        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Be($"There is no user with id {UserId2} in this home");
    }
}
using FluentAssertions;
using PhoeNix.Application.Mappings;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;

namespace PhoeNix.Application.UnitTests;

public class HomeMappingsTests
{
    [Fact]
    public void MapHomeToListDto_Should_Map_Correctly()
    {
        var homeId = new HomeId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());

        var home = Home.Create(homeId, "Test Home").Value;
        var homeUser = HomeUser.Create(new HomeUserId(Guid.NewGuid()), home.Id, userId).Value;
        home.SetHomeUser(homeUser);

        var result = HomeMappings.MapHomeToListDto(home);

        result.Should().NotBeNull();
        result.Id.Should().Be(home.Id);
        result.Name.Should().Be(home.Name);
        result.UserId.Should().Be(userId);
    }

    [Fact]
    public void MapHomeToDto_Should_Map_Correctly()
    {
        var homeId = new HomeId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());

        var user = User.Create(userId, "Test name", "Test description", "wheel", true, 1024, Shell.Fish, "./here").Value;
        var home = Home.Create(homeId, "Test Home").Value;

        var homeUser = HomeUser.Create(new HomeUserId(Guid.NewGuid()), home.Id, user.Id).Value;
        homeUser.SetUser(user);
        home.SetHomeUser(homeUser);

        var module = Module.Create(new ModuleId(Guid.NewGuid()), "mod", true, "", ModuleType.System,
            [Architecture.X86Linux]).Value;

        var homeModule = HomeModule.Create(new HomeModuleId(Guid.NewGuid()), home.Id, module.Id).Value;
        homeModule.SetModule(module);

        typeof(Home).GetField("_modules", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .SetValue(home, new List<HomeModule> { homeModule });

        var result = HomeMappings.MapHomeToDto(home);

        result.Should().NotBeNull();
        result.Id.Should().Be(home.Id);
        result.Name.Should().Be(home.Name);
        result.User.Id.Should().Be(user.Id);
        result.Modules.Should().ContainSingle(m => m.Id == module.Id);
    }
}
using FluentAssertions;
using PhoeNix.Domain.Entities.Homes;
using PhoeNix.Domain.Entities.Modules;
using PhoeNix.Domain.Entities.Users;
using PhoeNix.Domain.Enums;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class HomeRepositoryTests : PersistenceTestsBase
{
    public HomeRepositoryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task Should_Persist_Home_With_Users_And_Modules()
    {
        // Arrange
        var homeId = new HomeId(Guid.NewGuid());
        var home = Home.Create(homeId, "Test Home").Value;

        var module = Module.Create(
            new ModuleId(Guid.NewGuid()),
            "Analytics",
            true,
            "",
            ModuleType.Home,
            [Architecture.Aarch64Linux]
        ).Value;

        var user = User.Create(new UserId(Guid.NewGuid()), "Test name", "Test description", "wheel", true, 1024, Shell.Fish, "./here").Value;

        ModuleRepository.Add(module);
        UserRepository.Add(user);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        home.AddModule(module).IsSuccess.Should().BeTrue();
        home.AddUser(user).IsSuccess.Should().BeTrue();

        // Act
        HomeRepository.Add(home);
        await PhoeNixDbContextSUT.SaveChangesAsync();
        var retrieved = await HomeRepository.GetByIdAsync(home.Id, CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Name.Should().Be("Test Home");

        retrieved.Modules.Should().ContainSingle(m => m.Module.Id == module.Id);
        retrieved.Users.Should().ContainSingle(u => u.User.Id == user.Id);
    }

    [Fact]
    public async Task Should_Get_Home_By_Name_With_Relations()
    {
        // Arrange
        var homeId = new HomeId(Guid.NewGuid());
        var home = Home.Create(homeId, "FindMeHome").Value;

        var module = Module.Create(
            new ModuleId(Guid.NewGuid()),
            "Security",
            true,
            "",
            ModuleType.Home,
            [Architecture.X86Linux]
        ).Value;

        var user = User.Create(new UserId(Guid.NewGuid()), "Test name", "Test description", "wheel", true, 1024, Shell.Fish, "./here").Value;

        ModuleRepository.Add(module);
        UserRepository.Add(user);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        home.AddModule(module);
        home.AddUser(user);

        HomeRepository.Add(home);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        // Act
        var retrieved = await HomeRepository.GetByNameAsync("FindMe", CancellationToken.None);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Modules.Should().HaveCount(1);
        retrieved.Users.Should().HaveCount(1);
        retrieved.Modules[0].Module.Name.Should().Be("Security");
    }
}
using FluentAssertions;
using PhoeNix.Application.Repositories;
using PhoeNix.Domain.Entities.Users;
using Xunit.Abstractions;

namespace PhoeNix.Persistence.Tests;

public class UserRepositoryTests : PersistenceTestsBase
{
    public UserRepositoryTests(ITestOutputHelper output) : base(output)
    {
    }

    private static User CreateUser(string name)
    {
        var user = User.Create(new UserId(Guid.NewGuid()), name).Value;
        user.SetPasswordHash("hashed_password");
        return user;
    }

    [Fact]
    public async Task GetByNameAsync_Should_Return_User_With_Matching_Name()
    {
        var user = CreateUser("Alice");
        await PhoeNixDbContextSUT.Users.AddAsync(user);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await UserRepository.GetByNameAsync("Alice", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
    }

    [Fact]
    public async Task GetByNameAsync_Should_Return_User_For_Partial_Name()
    {
        var user = CreateUser("AliceSmith");
        await PhoeNixDbContextSUT.Users.AddAsync(user);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await UserRepository.GetByNameAsync("Smith", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("AliceSmith");
    }

    [Fact]
    public async Task GetByNameAsync_Should_Return_Null_When_No_Match()
    {
        var result = await UserRepository.GetByNameAsync("NonExistent", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByNormalizedNameAsync_Should_Return_User_For_Normalized_Name()
    {
        var user = CreateUser("Bob");
        await PhoeNixDbContextSUT.Users.AddAsync(user);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await UserRepository.GetByNormalizedNameAsync("BOB", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Bob");
    }

    [Fact]
    public async Task GetByNormalizedNameAsync_Should_Return_Null_When_No_Match()
    {
        var result = await UserRepository.GetByNormalizedNameAsync("NOBODY", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ExistsByNormalizedNameAsync_Should_Return_True_When_User_Exists()
    {
        var user = CreateUser("Charlie");
        await PhoeNixDbContextSUT.Users.AddAsync(user);
        await PhoeNixDbContextSUT.SaveChangesAsync();

        var result = await UserRepository.ExistsByNormalizedNameAsync("CHARLIE", CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsByNormalizedNameAsync_Should_Return_False_When_User_Does_Not_Exist()
    {
        var result = await UserRepository.ExistsByNormalizedNameAsync("GHOST", CancellationToken.None);

        result.Should().BeFalse();
    }
}
